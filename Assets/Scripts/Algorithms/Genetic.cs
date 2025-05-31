using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq; // For OrderBy

namespace Algorithms
{
    public static class Genetic
    {
        private class Chromosome
        {
            public List<Vector3Int> Path { get; set; }
            public float Fitness { get; set; }
            public int PathLength => Path?.Count ?? 0;

            public Chromosome(List<Vector3Int> path)
            {
                Path = path;
                Fitness = 0f;
            }

            // Copy constructor
            public Chromosome(Chromosome other)
            {
                Path = new List<Vector3Int>(other.Path);
                Fitness = other.Fitness;
            }
        }

        // GA Parameters - these can be tuned
        private static int _populationSize = 100;
        private static int _maxGenerations = 500;
        private static float _mutationRate = 0.15f; // Per-chromosome mutation probability
        private static float _geneMutationRate = 0.05f; // Per-gene (step in path) mutation probability
        private static int _tournamentSize = 7;
        private static int _elitismCount = 5;
        private static float manhattanDistance = 0.1f;
        private static int _maxPathLengthFactor = 2; // Max path length = manhattan_distance * factor

        public static List<Vector3Int> FindPath(Vector3Int startPos, Vector3Int endPos, Tilemap tilemap,
                                                int populationSize = 50, int maxGenerations = 100, 
                                                float mutationRate = 0.1f, int elitismCount = 2, 
                                                float geneMutationRate = 0.05f, int tournamentSize = 5)
        {
            _populationSize = populationSize;
            _maxGenerations = maxGenerations;
            _mutationRate = mutationRate;
            _elitismCount = elitismCount;
            _geneMutationRate = geneMutationRate;
            _tournamentSize = tournamentSize;


            if (!IsWalkable(startPos, tilemap) || !IsWalkable(endPos, tilemap))
            {
                Debug.LogError("[GeneticAlgorithm] Start or End position is not walkable.");
                return null;
            }

            List<Chromosome> population = InitializePopulation(startPos, endPos, tilemap);
            if (population == null || population.Count == 0)
            {
                 Debug.LogError("[GeneticAlgorithm] Failed to initialize population.");
                 return null;
            }

            for (int gen = 0; gen < _maxGenerations; gen++)
            {
                foreach (var chromosome in population)
                {
                    CalculateFitness(chromosome, startPos, endPos, tilemap);
                }

                population = population.OrderByDescending(c => c.Fitness).ToList();

                if (population[0].Path.Count > 0 && 
                    population[0].Path.Last().Equals(endPos) && 
                    population[0].Path.Count <= manhattanDistance * 1.2f) // Close to optimal
                {
                    Debug.Log($"Optimal solution found in generation {gen + 1}");
                    return ValidateAndCleanFinalPath(population[0].Path, startPos, endPos, tilemap);
                }


                List<Chromosome> newPopulation = new List<Chromosome>();

                for (int i = 0; i < _elitismCount && i < population.Count; i++)
                {
                    newPopulation.Add(new Chromosome(population[i]));
                }

                while (newPopulation.Count < _populationSize)
                {
                    Chromosome parent1 = SelectParent(population);
                    Chromosome parent2 = SelectParent(population);

                    Chromosome offspring1, offspring2;
                    Crossover(parent1, parent2, out offspring1, out offspring2);

                    Mutate(offspring1, startPos, endPos, tilemap);
                    Mutate(offspring2, startPos, endPos, tilemap);
                    
                    newPopulation.Add(offspring1);
                    if (newPopulation.Count < _populationSize)
                    {
                        newPopulation.Add(offspring2);
                    }
                }
                population = newPopulation;
                if (gen > 50) // Start checking after some generations
                {
                    float avgFitness = population.Take(10).Average(c => c.Fitness);
                    // Store previous generation's average and compare
                    // If improvement is minimal for several generations, break
                    Debug.Log($"[GeneticAlgorithm] Fitness: {avgFitness}");
                }

                Debug.Log($"[GeneticAlgorithm] Generation {gen + 1}: Best Fitness = {population[0].Fitness}, Path Length = {population[0].PathLength}, Ends at: {population[0].Path.LastOrDefault()}");
            }
            
            population = population.OrderByDescending(c => c.Fitness).ToList();
            if (population.Count > 0 && population[0].Fitness > 0.01f)
            {
                 Debug.LogWarning($"[GeneticAlgorithm] Finished. Best path fitness: {population[0].Fitness}. Path Length: {population[0].PathLength}. Path may not reach target or be optimal.");
                return ValidateAndCleanFinalPath(population[0].Path, startPos, endPos, tilemap);
            }

            Debug.LogWarning("[GeneticAlgorithm] Could not find a suitable path after " + _maxGenerations + " generations.");
            return null;
        }

        private static List<Chromosome> InitializePopulation(Vector3Int startPos, Vector3Int endPos, Tilemap tilemap)
        {
            List<Chromosome> population = new List<Chromosome>();
            int manhattanDist = CalculateManhattanDistance(startPos, endPos);
            int initialMaxPathLength = manhattanDist > 0 ? manhattanDist * _maxPathLengthFactor : 10; // Ensure a minimum length
            if (initialMaxPathLength < 5) initialMaxPathLength = 5;


            for (int i = 0; i < _populationSize; i++)
            {
                List<Vector3Int> path = new List<Vector3Int> { startPos };
                Vector3Int currentPos = startPos;
                
                // Attempt a random walk, can be improved with bias towards endPos
                for (int step = 0; step < initialMaxPathLength - 1; step++)
                {
                    if (currentPos.Equals(endPos) && path.Count > 1) break; // Reached end

                    List<Vector3Int> neighbors = GetWalkableNeighbors(currentPos, tilemap, null, true); // Allow re-visiting for initial random paths
                    if (neighbors.Count == 0) break; // Dead end

                    currentPos = neighbors[Random.Range(0, neighbors.Count)];
                    path.Add(currentPos);
                }
                population.Add(new Chromosome(path));
            }
            return population;
        }

        private static void CalculateFitness(Chromosome chromosome, Vector3Int startPos, Vector3Int endPos, Tilemap tilemap)
        {
            List<Vector3Int> path = chromosome.Path;
            if (path == null || path.Count == 0 || path[0] != startPos)
            {
                chromosome.Fitness = 0;
                return;
            }

            float fitness = 0f;
            Vector3Int actualEndPos = path.Last();
            int manhattanDistance = CalculateManhattanDistance(startPos, endPos);

            // Validate path contiguity and walkability
            Vector3Int previousPos = startPos;
            for (int i = 1; i < path.Count; i++)
            {
                Vector3Int currentPos = path[i];
                if (!IsWalkable(currentPos, tilemap) || !AreAdjacent(previousPos, currentPos))
                {
                    chromosome.Fitness = 0;
                    return;
                }
                previousPos = currentPos;
            }
    
            if (actualEndPos.Equals(endPos))
            {
                // Much stronger preference for shorter paths that reach the target
                float pathEfficiency = (float)manhattanDistance / path.Count;
                fitness = 10000f * pathEfficiency; // Higher base score with efficiency multiplier
            }
            else
            {
                // Lower fitness for paths that don't reach the target
                float distanceToEnd = CalculateManhattanDistance(actualEndPos, endPos);
                fitness = 100f / (1f + distanceToEnd + path.Count * 0.1f);
            }
    
            chromosome.Fitness = fitness;

        }

        private static Chromosome SelectParent(List<Chromosome> population) // Tournament Selection
        {
            Chromosome best = null;
            for (int i = 0; i < _tournamentSize; i++)
            {
                int randomIndex = Random.Range(0, population.Count);
                Chromosome randomInd = population[randomIndex];
                if (best == null || randomInd.Fitness > best.Fitness)
                {
                    best = randomInd;
                }
            }
            return new Chromosome(best); // Return a copy
        }

        private static void Crossover(Chromosome parent1, Chromosome parent2, out Chromosome offspring1, out Chromosome offspring2)
        {
            List<Vector3Int> p1Path = parent1.Path;
            List<Vector3Int> p2Path = parent2.Path;

            // Ensure paths are new lists for offspring
            List<Vector3Int> off1Path = new List<Vector3Int>();
            List<Vector3Int> off2Path = new List<Vector3Int>();

            if (p1Path.Count <= 1 || p2Path.Count <= 1) // Not enough material
            {
                offspring1 = new Chromosome(parent1); // Just copy parents
                offspring2 = new Chromosome(parent2);
                return;
            }

            int crossoverPoint1 = Random.Range(1, p1Path.Count);
            int crossoverPoint2 = Random.Range(1, p2Path.Count);

            off1Path.AddRange(p1Path.GetRange(0, crossoverPoint1));
            if (p2Path.Count > crossoverPoint2)
                off1Path.AddRange(p2Path.GetRange(crossoverPoint2, p2Path.Count - crossoverPoint2));
            
            off2Path.AddRange(p2Path.GetRange(0, crossoverPoint2));
            if (p1Path.Count > crossoverPoint1)
                off2Path.AddRange(p1Path.GetRange(crossoverPoint1, p1Path.Count - crossoverPoint1));

            offspring1 = new Chromosome(RemoveConsecutiveDuplicates(off1Path));
            offspring2 = new Chromosome(RemoveConsecutiveDuplicates(off2Path));
        }
        
        private static List<Vector3Int> RemoveConsecutiveDuplicates(List<Vector3Int> path)
        {
            if (path == null || path.Count <= 1) return path;
            
            List<Vector3Int> uniquePath = new List<Vector3Int> { path[0] };
            for (int i = 1; i < path.Count; i++)
            {
                if (path[i] != path[i-1])
                {
                    uniquePath.Add(path[i]);
                }
            }
            return uniquePath;
        }

        private static void Mutate(Chromosome chromosome, Vector3Int startPos, Vector3Int endPos, Tilemap tilemap)
        {
            if (Random.Range(0f, 1f) > _mutationRate) return;
            
            List<Vector3Int> path = chromosome.Path;
            if (path.Count <= 1) return; // Cannot mutate start point or empty path effectively

            for (int i = 1; i < path.Count; i++) // Start from 1 to protect startPos
            {
                if (Random.Range(0f, 1f) < _geneMutationRate)
                {
                    // Mutation: Change a step to a random walkable neighbor of the PREVIOUS step
                    Vector3Int previousStep = path[i-1];
                    List<Vector3Int> neighbors = GetWalkableNeighbors(previousStep, tilemap, null, true);
                    if (neighbors.Count > 0)
                    {
                        path[i] = neighbors[Random.Range(0, neighbors.Count)];
                    }
                }
            }
            
            // Ensure path always starts at startPos after potential mutations
            if (path.Count == 0 || path[0] != startPos)
            {
                path.Insert(0, startPos);
                chromosome.Path = RemoveConsecutiveDuplicates(path); // Clean up if startPos was re-added
            }
            
            // Trim path if it goes way too long after mutations
            int manhattanDist = CalculateManhattanDistance(startPos, endPos);
            int maxAllowedLength = manhattanDist > 0 ? manhattanDist * (_maxPathLengthFactor + 2) : 20; // A bit more lenient for mutations
            if (path.Count > maxAllowedLength) {
                path.RemoveRange(maxAllowedLength, path.Count - maxAllowedLength);
            }
        }

        private static bool IsWalkable(Vector3Int pos, Tilemap tilemap)
        {
            return tilemap.HasTile(pos);
        }

        private static bool AreAdjacent(Vector3Int posA, Vector3Int posB)
        {
            int deltaX = Mathf.Abs(posA.x - posB.x);
            int deltaY = Mathf.Abs(posA.y - posB.y);
            return (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
        }

        private static List<Vector3Int> GetWalkableNeighbors(Vector3Int currentPos, Tilemap tilemap, List<Vector3Int> pathBeingBuilt, bool allowRevisitInRandomWalk = false)
        {
            List<Vector3Int> neighbors = new List<Vector3Int>();
            Vector3Int[] directions = 
            {
                new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
                new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0)
            };

            foreach (Vector3Int dir in directions)
            {
                Vector3Int neighborPos = currentPos + dir;
                if (IsWalkable(neighborPos, tilemap))
                {
                    if (allowRevisitInRandomWalk || pathBeingBuilt == null || !pathBeingBuilt.Contains(neighborPos))
                    {
                         neighbors.Add(neighborPos);
                    }
                }
            }
            return neighbors;
        }
        
        private static int CalculateManhattanDistance(Vector3Int a, Vector3Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        private static List<Vector3Int> ValidateAndCleanFinalPath(List<Vector3Int> path, Vector3Int startPos, Vector3Int endPos, Tilemap tilemap)
        {
            if (path == null || path.Count == 0) return new List<Vector3Int>{ startPos };

            List<Vector3Int> finalPath = new List<Vector3Int>();
            
            // Ensure path starts with startPos
            if (path[0] != startPos)
            {
                finalPath.Add(startPos);
            }
            
            Vector3Int current = finalPath.Count > 0 ? finalPath[0] : path[0]; // Start from startPos or first element
            if (finalPath.Count == 0) finalPath.Add(current);


            for (int i = 1; i < path.Count; i++) // Start from the first element if startPos was prepended, or second if not
            {
                Vector3Int nextPoint = path[i];
                if (nextPoint == current) continue; // Skip duplicates

                if (IsWalkable(nextPoint, tilemap) && AreAdjacent(current, nextPoint))
                {
                    finalPath.Add(nextPoint);
                    current = nextPoint;
                    if (current == endPos) break; 
                }
                // else: Path segment is invalid (not walkable or not adjacent).
                // The path will be truncated here or skip this invalid segment.
                // For a more robust solution, one might try to find a short path
                // between `current` and a later valid point in `path`.
            }
            
            return RemoveConsecutiveDuplicates(finalPath);
        }
    }
}