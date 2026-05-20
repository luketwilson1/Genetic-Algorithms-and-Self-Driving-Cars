using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

public class GeneticManager : MonoBehaviour
{
    [Header("References")]
    public CarController controller;

    [Header("Controls")]
    public int initialPopulation = 85;
    [Range(0.0f, 1.0f)]
    public float mutationRate = 0.055f;
    [Range(1f, 20f)]
    public float timeScale = 1f;

    [Header("Crossover Controls")]
    public int bestAgents = 8;
    public int worstAgents = 3;
    public int numberToCrossover;

    [Header("Public View")]
    public int currentGeneration;
    public int currentGenome = 0;

    private readonly List<int> genePool = new List<int>();
    private float defaultFixedDeltaTime;
    private int naturallySelected;
    private NeuralNetwork[] population;

    private void Awake()
    {
        defaultFixedDeltaTime = Time.fixedDeltaTime;
        ApplyTimeScale();
    }

    private void Start()
    {
        CreatePopulation();
    }

    private void Update()
    {
        ApplyTimeScale();
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
    }

    private void OnValidate()
    {
        timeScale = Mathf.Max(0.1f, timeScale);
    }

    private void ApplyTimeScale()
    {
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = defaultFixedDeltaTime * timeScale;
    }

    private void CreatePopulation()
    {
        population = new NeuralNetwork[initialPopulation];
        RandomizePopulationValues(population, 0);
        ResetGenome();
    }

    private void ResetGenome()
    {
        controller.ResetNetwork(population[currentGenome]);
    }

    private void RandomizePopulationValues(NeuralNetwork[] newPopulation, int startingIndex)
    {
        while (startingIndex < initialPopulation)
        {
            newPopulation[startingIndex] = new NeuralNetwork();
            newPopulation[startingIndex].Initialize(controller.layers, controller.neurons);
            startingIndex++;
        }
    }

    public void Death(float fitness, NeuralNetwork network)
    {
        if (currentGenome < population.Length - 1)
        {
            population[currentGenome].fitness = fitness;
            currentGenome++;
            ResetGenome();
        }
        else
        {
            Repopulate();
        }
    }

    private void Repopulate()
    {
        genePool.Clear();
        currentGeneration++;
        naturallySelected = 0;
        SortPopulation();

        NeuralNetwork[] newPopulation = PickBestPopulation();

        Crossover(newPopulation);
        Mutate(newPopulation);
        RandomizePopulationValues(newPopulation, naturallySelected);

        population = newPopulation;
        currentGenome = 0;

        ResetGenome();
    }

    private void Mutate(NeuralNetwork[] newPopulation)
    {
        for (int i = 0; i < naturallySelected; i++)
        {
            for (int c = 0; c < newPopulation[i].weights.Count; c++)
            {
                if (Random.Range(0.0f, 1.0f) < mutationRate)
                {
                    newPopulation[i].weights[c] = MutateMatrix(newPopulation[i].weights[c]);
                }
            }
        }
    }

    private Matrix<float> MutateMatrix(Matrix<float> matrix)
    {
        int randomPoints = Random.Range(1, (matrix.RowCount * matrix.ColumnCount) / 7);
        Matrix<float> mutatedMatrix = matrix;

        for (int i = 0; i < randomPoints; i++)
        {
            int randomColumn = Random.Range(0, mutatedMatrix.ColumnCount);
            int randomRow = Random.Range(0, mutatedMatrix.RowCount);

            mutatedMatrix[randomRow, randomColumn] = Mathf.Clamp(
                mutatedMatrix[randomRow, randomColumn] + Random.Range(-1f, 1f),
                -1f,
                1f);
        }

        return mutatedMatrix;
    }

    private void Crossover(NeuralNetwork[] newPopulation)
    {
        for (int i = 0; i < numberToCrossover; i += 2)
        {
            int aIndex = i;
            int bIndex = i + 1;

            if (genePool.Count >= 1)
            {
                for (int l = 0; l < 100; l++)
                {
                    aIndex = genePool[Random.Range(0, genePool.Count)];
                    bIndex = genePool[Random.Range(0, genePool.Count)];

                    if (aIndex != bIndex)
                    {
                        break;
                    }
                }
            }

            NeuralNetwork child1 = new NeuralNetwork();
            NeuralNetwork child2 = new NeuralNetwork();

            child1.Initialize(controller.layers, controller.neurons);
            child2.Initialize(controller.layers, controller.neurons);

            child1.fitness = 0;
            child2.fitness = 0;

            for (int w = 0; w < child1.weights.Count; w++)
            {
                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    child1.weights[w] = population[aIndex].weights[w];
                    child2.weights[w] = population[bIndex].weights[w];
                }
                else
                {
                    child2.weights[w] = population[aIndex].weights[w];
                    child1.weights[w] = population[bIndex].weights[w];
                }
            }

            for (int w = 0; w < child1.biases.Count; w++)
            {
                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    child1.biases[w] = population[aIndex].biases[w];
                    child2.biases[w] = population[bIndex].biases[w];
                }
                else
                {
                    child2.biases[w] = population[aIndex].biases[w];
                    child1.biases[w] = population[bIndex].biases[w];
                }
            }

            newPopulation[naturallySelected] = child1;
            naturallySelected++;

            newPopulation[naturallySelected] = child2;
            naturallySelected++;
        }
    }

    private NeuralNetwork[] PickBestPopulation()
    {
        NeuralNetwork[] newPopulation = new NeuralNetwork[initialPopulation];

        for (int i = 0; i < bestAgents; i++)
        {
            newPopulation[naturallySelected] = population[i].InitializeCopy(controller.layers, controller.neurons);
            newPopulation[naturallySelected].fitness = 0;
            naturallySelected++;

            int f = Mathf.RoundToInt(population[i].fitness * 10);

            for (int c = 0; c < f; c++)
            {
                genePool.Add(i);
            }
        }

        for (int i = 0; i < worstAgents; i++)
        {
            int last = population.Length - 1;
            last -= i;

            int f = Mathf.RoundToInt(population[last].fitness * 10);

            for (int c = 0; c < f; c++)
            {
                genePool.Add(last);
            }
        }

        return newPopulation;
    }

    private void SortPopulation()
    {
        for (int i = 0; i < population.Length; i++)
        {
            for (int j = i; j < population.Length; j++)
            {
                if (population[i].fitness < population[j].fitness)
                {
                    NeuralNetwork temp = population[i];
                    population[i] = population[j];
                    population[j] = temp;
                }
            }
        }
    }
}
