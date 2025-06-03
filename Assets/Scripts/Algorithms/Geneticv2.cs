using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

namespace Algorithms
{
    public static class Geneticv2
    {
        public class StepData
        {
            public List<Vector3Int> currentPath;
            public List<Vector3Int> exploredNeighbors;
            public Vector3Int currentNode;
            public int generation;
            public double bestFitness;
            public bool reachedTarget;

            public StepData(List<Vector3Int> path, List<Vector3Int> neighbors, Vector3Int current, int gen, double fitness, bool reached)
            {
                this.currentPath = path;
                this.exploredNeighbors = neighbors;
                this.currentNode = current;
                this.generation = gen;
                this.bestFitness = fitness;
                this.reachedTarget = reached;
            }
        }

        // --- GA Parameters ---
        private static int populationSize = 100;       // Number of individuals in the population
        private static int maxGenerations = 200;       // Number of generations to evolve
        private static double crossoverRate = 0.8;     // Probability of crossover
        private static double mutationRate = 0.1;      // Probability of mutation per gene
        private static int tournamentSize = 5;         // Size of the tournament for selection
        private static int elitismCount = 2;           // Number of best individuals to carry to next generation
        private static int maxPathSegmentLength;       // Max length of the gene sequence (moves)

        private static System.Random random = new System.Random();

        // Possible moves (No diagonals)
        private enum Move { Up, Down, Left, Right, None } // 'None' can be an option for variable length paths or as a neutral gene
        private static Vector3Int[] moveVectors = {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right,
            Vector3Int.zero // For Move.None
        };

        // Create array of valid moves excluding None
        private static readonly Move[] validMoves = { Move.Up, Move.Down, Move.Left, Move.Right };

        private class Individual
        {
            public List<Move> Genes { get; set; }
            public double Fitness { get; set; }
            public List<Vector3Int> DecodedPath { get; set; } // Actual path taken
            public bool ReachedTarget { get; set; }

            public Individual(int geneLength)
            {
                Genes = new List<Move>(geneLength);
                for (int i = 0; i < geneLength; i++)
                {
                    Genes.Add(validMoves[random.Next(validMoves.Length)]);
                }
                Fitness = 0;
                DecodedPath = new List<Vector3Int>();
                ReachedTarget = false;
            }

            public Individual(List<Move> genes) // For offspring
            {
                Genes = new List<Move>(genes);
                Fitness = 0;
                DecodedPath = new List<Vector3Int>();
                ReachedTarget = false;
            }
        }

        // Helper to check if a tile is walkable
        // This is a simple check: assumes any tile present is walkable, and null is not.
        // You might need to customize this based on your tile setup (e.g., specific obstacle tiles).
        private static bool IsWalkable(Vector3Int position)
        {
            // If you have specific "ground" tiles and "obstacle" tiles:
            // TileBase tile = tilemap.GetTile(position);
            // if (tile == null) return false; // Or true if empty space is walkable
            // return !IsObstacleTile(tile); // You'd need an IsObstacleTile function

            // Simple version: if a tile exists, it's walkable. If outside map or no tile, not walkable.
            return GridManager.instance.IsWalkable(position);
        }

        private static int ManhattanDistance(Vector3Int a, Vector3Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        private static void DecodeAndEvaluate(Individual individual, Vector3Int startTile, Vector3Int endTile)
        {
            individual.DecodedPath.Clear();
            individual.DecodedPath.Add(startTile);
            individual.ReachedTarget = false;
            Vector3Int currentPos = startTile;
            HashSet<Vector3Int> visitedInPath = new HashSet<Vector3Int> { startTile }; // To penalize self-intersections

            int selfIntersections = 0;

            for (int i = 0; i < individual.Genes.Count; i++)
            {
                Move move = individual.Genes[i];
                if (move == Move.None) continue; // Skip 'None' moves

                Vector3Int nextPos = currentPos + moveVectors[(int)move];

                if (!IsWalkable(nextPos))
                {
                    // Hit an obstacle or went off map, path ends here
                    break;
                }

                if (visitedInPath.Contains(nextPos))
                {
                    selfIntersections++; // Penalize stepping on own path
                }

                currentPos = nextPos;
                individual.DecodedPath.Add(currentPos);
                visitedInPath.Add(currentPos);

                if (currentPos == endTile)
                {
                    individual.ReachedTarget = true;
                    break; // Reached target
                }
            }

            // Fitness Calculation
            double fitness = 0;
            int distanceToTarget = ManhattanDistance(currentPos, endTile);

            if (individual.ReachedTarget)
            {
                // Higher fitness for reaching target, much higher for shorter paths
                fitness = 10000.0 - (individual.DecodedPath.Count * 10.0); // Strong penalty for length
            }
            else
            {
                // Did not reach target, fitness based on how close it got
                // The '1.0 +' avoids division by zero if distance is 0 but target not "officially" reached
                fitness = 1.0 / (1.0 + distanceToTarget);
            }

            // Penalize self-intersections more heavily if target not reached
            fitness -= selfIntersections * (individual.ReachedTarget ? 0.1 : 1.0);

            // Penalize paths that are just wiggling at the start
            if (individual.DecodedPath.Count < 2 && !individual.ReachedTarget)
            {
                fitness *= 0.1; // Heavily penalize paths that didn't move or only moved one step and failed
            }

            individual.Fitness = Mathf.Max(0, (float)fitness); // Ensure fitness is not negative
        }

        public static IEnumerator FindPath(Vector3Int startTile, Vector3Int endTile)
        {
            // --- Basic Sanity Checks ---
            if (!IsWalkable(startTile)) yield break; // Start is an obstacle
            if (!IsWalkable(endTile)) yield break;   // End is an obstacle
            if (startTile == endTile)
            {
                yield return new StepData(
                    new List<Vector3Int> { startTile },
                    new List<Vector3Int>(),
                    startTile,
                    0,
                    10000.0,
                    true
                );
                yield break;
            }

            // Heuristic for max path segment length
            maxPathSegmentLength = Mathf.CeilToInt(ManhattanDistance(startTile, endTile) * 1.8f);
            if (maxPathSegmentLength < 5) maxPathSegmentLength = 5; // Minimum length
            Debug.Log($"Max Path Segment Length set to: {maxPathSegmentLength}");

            // At top of FindPath, after computing maxPathSegmentLength, add lower bound variable
            int optimalLowerBound = ManhattanDistance(startTile, endTile);

            // --- 1. Initialize Population ---
            List<Individual> population = new List<Individual>(populationSize);
            for (int i = 0; i < populationSize; i++)
            {
                population.Add(new Individual(maxPathSegmentLength));
            }

            Individual bestOverallIndividual = null;

            // --- 2. Evolution Loop ---
            for (int gen = 0; gen < maxGenerations; gen++)
            {
                // --- a. Evaluate Fitness ---
                foreach (var individual in population)
                {
                    DecodeAndEvaluate(individual, startTile, endTile);
                }

                // Sort population by fitness (descending)
                population = population.OrderByDescending(ind => ind.Fitness).ToList();

                // Update best overall solution found so far
                if (bestOverallIndividual == null || population[0].Fitness > bestOverallIndividual.Fitness)
                {
                    bestOverallIndividual = new Individual(new List<Move>(population[0].Genes))
                    {
                        Fitness = population[0].Fitness,
                        DecodedPath = new List<Vector3Int>(population[0].DecodedPath),
                        ReachedTarget = population[0].ReachedTarget
                    };
                }

                // Yield step data for visualization
                yield return new StepData(
                    population[0].DecodedPath,
                    GetExploredNeighbors(population[0].DecodedPath),
                    population[0].DecodedPath.Count > 0 ? population[0].DecodedPath.Last() : startTile,
                    gen,
                    population[0].Fitness,
                    population[0].ReachedTarget
                );

                // --- b. Check for Solution ---
                if (population[0].ReachedTarget)
                {
                    // If best path reaches target and is optimal (equals lower bound), we can stop early.
                    if (population[0].DecodedPath.Count == optimalLowerBound)
                    {
                        Debug.Log($"Optimal shortest path found in generation {gen + 1}. Length: {population[0].DecodedPath.Count}");
                        // Update bestOverallIndividual to current best and break outer loop
                        bestOverallIndividual = new Individual(new List<Move>(population[0].Genes))
                        {
                            Fitness = population[0].Fitness,
                            DecodedPath = new List<Vector3Int>(population[0].DecodedPath),
                            ReachedTarget = true
                        };
                        break; // exit evolution loop
                    }
                }

                // --- c. Create New Generation ---
                List<Individual> newPopulation = new List<Individual>(populationSize);

                // Elitism: Carry over the best individuals
                for (int i = 0; i < elitismCount && i < population.Count; i++)
                {
                    newPopulation.Add(new Individual(new List<Move>(population[i].Genes)));
                }

                // Fill the rest of the new population
                while (newPopulation.Count < populationSize)
                {
                    Individual parent1 = TournamentSelection(population);
                    Individual parent2 = TournamentSelection(population);

                    Individual child1, child2;

                    if (random.NextDouble() < crossoverRate)
                    {
                        Crossover(parent1, parent2, out child1, out child2);
                    }
                    else
                    {
                        child1 = new Individual(new List<Move>(parent1.Genes));
                        child2 = new Individual(new List<Move>(parent2.Genes));
                    }

                    Mutate(child1);
                    Mutate(child2);

                    newPopulation.Add(child1);
                    if (newPopulation.Count < populationSize)
                    {
                        newPopulation.Add(child2);
                    }
                }
                population = newPopulation;

                if (gen % 20 == 0)
                {
                    Debug.Log($"Generation {gen + 1} | Best Fitness: {population[0].Fitness} | Reached Target: {population[0].ReachedTarget} | Path Length: {population[0].DecodedPath.Count} | Best Overall Fitness: {bestOverallIndividual?.Fitness} Reached: {bestOverallIndividual?.ReachedTarget}");
                }
            }

            // --- 3. After Loop: Return best solution found ---
            if (bestOverallIndividual != null)
            {
                DecodeAndEvaluate(bestOverallIndividual, startTile, endTile);
                if (bestOverallIndividual.ReachedTarget)
                {
                    Debug.Log($"GA finished. Best path found. Length: {bestOverallIndividual.DecodedPath.Count}");
                    yield return new StepData(
                        bestOverallIndividual.DecodedPath,
                        GetExploredNeighbors(bestOverallIndividual.DecodedPath),
                        bestOverallIndividual.DecodedPath.Count > 0 ? bestOverallIndividual.DecodedPath.Last() : startTile,
                        maxGenerations,
                        bestOverallIndividual.Fitness,
                        true
                    );
                }
            }

            Debug.Log("GA finished. No path found that reaches the target, or best path didn't reach.");
        }

        private static List<Vector3Int> GetExploredNeighbors(List<Vector3Int> path)
        {
            if (path == null || path.Count == 0) return new List<Vector3Int>();

            List<Vector3Int> neighbors = new List<Vector3Int>();
            Vector3Int lastPos = path.Last();

            foreach (Vector3Int dir in moveVectors)
            {
                if (dir == Vector3Int.zero) continue; // Skip the None move
                Vector3Int neighborPos = lastPos + dir;
                if (IsWalkable(neighborPos))
                {
                    neighbors.Add(neighborPos);
                }
            }

            return neighbors;
        }

        private static Individual TournamentSelection(List<Individual> currentPopulation)
        {
            Individual bestInTournament = null;
            for (int i = 0; i < tournamentSize; i++)
            {
                Individual contender = currentPopulation[random.Next(currentPopulation.Count)];
                if (bestInTournament == null || contender.Fitness > bestInTournament.Fitness)
                {
                    bestInTournament = contender;
                }
            }
            return bestInTournament;
        }

        private static void Crossover(Individual parent1, Individual parent2, out Individual child1, out Individual child2)
        {
            List<Move> parent1Genes = parent1.Genes;
            List<Move> parent2Genes = parent2.Genes;
            int geneLength = parent1Genes.Count; // Assume same length

            List<Move> child1Genes = new List<Move>(geneLength);
            List<Move> child2Genes = new List<Move>(geneLength);

            // Single-point crossover
            int crossoverPoint = random.Next(1, geneLength - 1); // Avoid ends for meaningful crossover

            for (int i = 0; i < geneLength; i++)
            {
                if (i < crossoverPoint)
                {
                    child1Genes.Add(parent1Genes[i]);
                    child2Genes.Add(parent2Genes[i]);
                }
                else
                {
                    child1Genes.Add(parent2Genes[i]);
                    child2Genes.Add(parent1Genes[i]);
                }
            }
            child1 = new Individual(child1Genes);
            child2 = new Individual(child2Genes);
        }

        private static void Mutate(Individual individual)
        {
            for (int i = 0; i < individual.Genes.Count; i++)
            {
                if (random.NextDouble() < mutationRate)
                {
                    // Change to a random different move (excluding Move.None for active mutation)
                    Move currentMove = individual.Genes[i];
                    Move newMovie;
                    do
                    {
                        newMovie = (Move)random.Next(0, moveVectors.Length - 1); // Exclude 'None'
                    } while (newMovie == currentMove && (moveVectors.Length - 1) > 1); // ensure change if possible
                    individual.Genes[i] = newMovie;
                }
            }
        }
    }
}