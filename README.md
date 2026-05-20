# Genetic Algorithm Self-Driving Car (Updates)

A Unity self-driving car simulation that trains an artificial neural network with a genetic algorithm. Each car reads three raycast sensors, feeds those sensor values into a neural network, and uses the network output to control acceleration and steering. Better-performing networks are selected, crossed over, mutated, and reused in the next generation.

## Project Overview

This project is built around three main scripts:

- `CarController.cs` controls the car, gathers sensor data, calculates fitness, and reports death events.
- `NeuralNetwork.cs` stores and runs the artificial neural network.
- `GeneticManager.cs` manages the population, selection, crossover, mutation, generations, and training speed.

## Requirements

- Unity 6.4 or newer
- MathNet Numerics for matrix operations

## How It Works

1. `GeneticManager` creates an initial population of neural networks.
2. The active network is sent to `CarController`.
3. The car reads left, center, and right raycast sensors.
4. Sensor values are passed into `NeuralNetwork.RunNetwork`.
5. The network returns acceleration and steering values.
6. The car drives until it crashes, performs poorly for too long, or reaches the fitness goal.
7. The manager stores the network fitness and moves to the next genome.
8. When the generation ends, the best networks are selected, crossed over, mutated, and used to create the next generation.

## CarController Properties

### Driving Output

| Property | Description |
| --- | --- |
| `accelerationInput` | The current acceleration value returned by the neural network. Ranges from `-1` to `1` in the Inspector. |
| `steeringInput` | The current steering value returned by the neural network. Ranges from `-1` to `1` in the Inspector. |
| `timeSinceStart` | Tracks how long the current car/network has been alive. |

### Fitness

| Property | Description |
| --- | --- |
| `overallFitness` | Final score used by the genetic algorithm to rank this network. |
| `distanceMultipler` | How strongly distance traveled affects fitness. Higher values reward cars for moving farther. |
| `avgSpeedMultiplier` | How strongly average speed affects fitness. Higher values reward faster driving. |
| `sensorMultiplier` | How strongly sensor distance affects fitness. Higher values reward cars that stay farther from walls. |

### Network Options

| Property | Description |
| --- | --- |
| `layers` | Number of hidden layer groups used when creating each neural network. |
| `neurons` | Number of neurons in each hidden layer. Higher values can learn more complex behavior but train slower. |

### Private Runtime Values

| Variable | Description |
| --- | --- |
| `network` | The active neural network currently controlling the car. |
| `startPosition` | The car position used when resetting a genome. |
| `startRotation` | The car rotation used when resetting a genome. |
| `lastPosition` | Previous frame position used to calculate distance traveled. |
| `totalDistanceTravelled` | Total distance driven by the current genome. |
| `avgSpeed` | Average speed for the current genome. |
| `aSensor` | Right-forward raycast sensor distance. |
| `bSensor` | Forward raycast sensor distance. |
| `carSensor` | Left-forward raycast sensor distance. |
| `interpolate` | Smoothed movement vector used by `MoveCar`. |

## GeneticManager Properties

### References

| Property | Description |
| --- | --- |
| `controller` | Reference to the active `CarController` in the scene. The manager sends neural networks to this car. |

### Controls

| Property | Description |
| --- | --- |
| `initialPopulation` | Number of neural networks created per generation. Larger populations give more variety but take longer to evaluate. |
| `mutationRate` | Chance that a network weight matrix will mutate. Values closer to `1` mutate more often. |
| `timeScale` | Speeds up or slows down training by changing Unity's `Time.timeScale`. Current Inspector range is `1x` to `20x`. |

### Crossover Controls

| Property | Description |
| --- | --- |
| `bestAgents` | Number of top-performing networks copied into the next generation. |
| `worstAgents` | Number of low-performing networks still allowed into the gene pool for variety. |
| `numberToCrossover` | Number of child networks created through crossover each generation. |

### Public View

| Property | Description |
| --- | --- |
| `currentGeneration` | Current generation number. Increases after every full population has been evaluated. |
| `currentGenome` | Index of the network currently being tested. |

### Private Runtime Values

| Variable | Description |
| --- | --- |
| `genePool` | Weighted list of selectable network indexes used during crossover. Better networks are added more often. |
| `defaultFixedDeltaTime` | Stores Unity's original fixed timestep so it can be restored when the manager is disabled. |
| `naturallySelected` | Tracks how many slots in the next population have already been filled. |
| `population` | Array of all neural networks in the current generation. |

## NeuralNetwork Properties

| Property | Description |
| --- | --- |
| `inputLayer` | Matrix containing the three sensor inputs. |
| `hiddenLayers` | List of hidden layer matrices used during network calculation. |
| `outputLayer` | Matrix containing the two output values: acceleration and steering. |
| `weights` | List of weight matrices that connect each layer of the network. |
| `biases` | List of bias values added while calculating layer outputs. |
| `fitness` | Score assigned by the car and used by the genetic algorithm. |

## Key Methods

### `CarController`

| Method | Purpose |
| --- | --- |
| `ResetNetwork` | Assigns a neural network to the car and resets the car state. |
| `Reset` | Moves the car back to its starting position and clears fitness tracking. |
| `InputSensors` | Casts three rays and stores normalized sensor distances. |
| `MoveCar` | Moves and turns the car from neural network output values. |
| `CalculateFitness` | Scores the car based on distance, speed, and sensor distance. |
| `Death` | Sends the final fitness score back to `GeneticManager`. |

### `GeneticManager`

| Method | Purpose |
| --- | --- |
| `CreatePopulation` | Creates the first generation of neural networks. |
| `ResetGenome` | Sends the current genome to the car. |
| `Repopulate` | Builds the next generation after all genomes have been tested. |
| `PickBestPopulation` | Copies the strongest networks and builds the gene pool. |
| `Crossover` | Creates child networks from two parent networks. |
| `Mutate` | Randomly changes network weights based on `mutationRate`. |
| `SortPopulation` | Orders networks by fitness from best to worst. |
| `ApplyTimeScale` | Applies the current training speed to Unity time. |

### `NeuralNetwork`

| Method | Purpose |
| --- | --- |
| `Initialize` | Builds the network layers, weights, and biases. |
| `InitializeCopy` | Creates a copy of an existing network for the next generation. |
| `InitializeHidden` | Rebuilds hidden layers when copying a network. |
| `RandomizeWeights` | Assigns random starting weights. |
| `RunNetwork` | Processes sensor inputs and returns acceleration and steering outputs. |

## Tuning Tips

- Increase `initialPopulation` for more variety, but expect slower generation cycles.
- Increase `mutationRate` if learning gets stuck, but lower it if behavior becomes too random.
- Increase `layers` or `neurons` for a more capable network, but keep values modest while testing.
- Use `timeScale` to speed up training once the simulation is stable.
- Tune `distanceMultipler`, `avgSpeedMultiplier`, and `sensorMultiplier` depending on whether you want the car to prioritize distance, speed, or avoiding walls.

## Notes

This project uses a simple feed-forward neural network and genetic algorithm. It does not use backpropagation. Learning happens through population selection, crossover, and mutation across generations.
