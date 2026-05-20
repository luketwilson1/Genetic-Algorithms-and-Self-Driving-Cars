using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;
using Random = UnityEngine.Random;

public class NeuralNetwork
{
    public Matrix<float> inputLayer = Matrix<float>.Build.Dense(1, 3);
    public List<Matrix<float>> hiddenLayers = new List<Matrix<float>>();
    public Matrix<float> outputLayer = Matrix<float>.Build.Dense(1, 2);
    public List<Matrix<float>> weights = new List<Matrix<float>>();
    public List<float> biases = new List<float>();
    public float fitness;

    public void Initialize(int numHiddenLayer, int numHiddenNeuron)
    {
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();
        weights.Clear();
        biases.Clear();

        for (int i = 0; i < numHiddenLayer + 1; i++)
        {
            Matrix<float> hiddenLayer = Matrix<float>.Build.Dense(1, numHiddenNeuron);
            hiddenLayers.Add(hiddenLayer);
            biases.Add(Random.Range(-1f, 1f));

            if (i == 0)
            {
                Matrix<float> inputToH1 = Matrix<float>.Build.Dense(3, numHiddenNeuron);
                weights.Add(inputToH1);
            }

            Matrix<float> hiddenToHidden = Matrix<float>.Build.Dense(numHiddenNeuron, numHiddenNeuron);
            weights.Add(hiddenToHidden);
        }

        Matrix<float> outputWeight = Matrix<float>.Build.Dense(numHiddenNeuron, 2);
        weights.Add(outputWeight);
        biases.Add(Random.Range(-1f, 1f));

        RandomizeWeights();
    }

    public void Initialise(int numHiddenLayer, int numHiddenNeuron)
    {
        Initialize(numHiddenLayer, numHiddenNeuron);
    }

    public NeuralNetwork InitialiseCopy(int hiddenLayerCount, int hiddenNeuronCount)
    {
        NeuralNetwork network = new NeuralNetwork();
        List<Matrix<float>> newWeights = new List<Matrix<float>>();

        for (int i = 0; i < weights.Count; i++)
        {
            Matrix<float> currentWeight = Matrix<float>.Build.Dense(weights[i].RowCount, weights[i].ColumnCount);

            for (int x = 0; x < currentWeight.RowCount; x++)
            {
                for (int y = 0; y < currentWeight.ColumnCount; y++)
                {
                    currentWeight[x, y] = weights[i][x, y];
                }
            }

            newWeights.Add(currentWeight);
        }

        List<float> newBiases = new List<float>();
        newBiases.AddRange(biases);

        network.weights = newWeights;
        network.biases = newBiases;
        network.InitializeHidden(hiddenLayerCount, hiddenNeuronCount);

        return network;
    }

    public NeuralNetwork InitializeCopy(int hiddenLayerCount, int hiddenNeuronCount)
    {
        return InitialiseCopy(hiddenLayerCount, hiddenNeuronCount);
    }

    public void InitializeHidden(int numHiddenLayer, int numHiddenNeuron)
    {
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();

        for (int i = 0; i < numHiddenLayer + 1; i++)
        {
            Matrix<float> newHiddenLayer = Matrix<float>.Build.Dense(1, numHiddenNeuron);
            hiddenLayers.Add(newHiddenLayer);
        }
    }

    public void InitialiseHidden(int numHiddenLayer, int numHiddenNeuron)
    {
        InitializeHidden(numHiddenLayer, numHiddenNeuron);
    }

    public void RandomizeWeights()
    {
        for (int i = 0; i < weights.Count; i++)
        {
            for (int x = 0; x < weights[i].RowCount; x++)
            {
                for (int y = 0; y < weights[i].ColumnCount; y++)
                {
                    weights[i][x, y] = Random.Range(-1f, 1f);
                }
            }
        }
    }

    public (float, float) RunNetwork(float a, float b, float c)
    {
        inputLayer[0, 0] = a;
        inputLayer[0, 1] = b;
        inputLayer[0, 2] = c;

        inputLayer = inputLayer.PointwiseTanh();
        hiddenLayers[0] = ((inputLayer * weights[0]) + biases[0]).PointwiseTanh();

        for (int i = 1; i < hiddenLayers.Count; i++)
        {
            hiddenLayers[i] = ((hiddenLayers[i - 1] * weights[i]) + biases[i]).PointwiseTanh();
        }

        outputLayer = ((hiddenLayers[hiddenLayers.Count - 1] * weights[weights.Count - 1]) + biases[biases.Count - 1]).PointwiseTanh();

        return (Sigmoid(outputLayer[0, 0]), (float)Math.Tanh(outputLayer[0, 1]));
    }

    private float Sigmoid(float s)
    {
        return 1 / (1 + Mathf.Exp(-s));
    }
}
