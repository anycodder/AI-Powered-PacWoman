using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class GeneticAlgorithm : MonoBehaviour
{
    private List<Chrom> population;
    private string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
    private string fileName;
    public Functions functions;
    public GameManager gameManager;
    private int lastScore;
    private int topChromCount = 30; 
    private float mutationRate = 0.3f;
    private string filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "FitnessResults.txt");
    private string filePath2 = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "FitnessResultsDONTTAKE.txt");
    private int currentGeneration = 0;
    private int maxGenerations = 20; 
    private int offspringCount = 20;


    private void Awake()
    {
        functions = FindObjectOfType<Functions>();
        gameManager = FindObjectOfType<GameManager>();
    }

    void Start()
    {
        population = ReadChromsFromFile(filePath);
        fileName = "FitnessResults_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt"; 
        //Debug.Log("Starting Evolution Coroutine");
        StartCoroutine(RunEvolution());
    }

    void Update()
    {
        lastScore = gameManager.score;
    }

    IEnumerator RunEvolution()
    {
        while (currentGeneration < maxGenerations)
        {
            
            List<Chrom> matingPool = SelectParents(population);
            List<Chrom> offspring = Crossover(matingPool);
            List<Chrom> newPopulation = Mutate(offspring, currentGeneration, maxGenerations);

            
            for (int i = 0; i < newPopulation.Count; i++)
            {
                StartGameWithChrom(newPopulation[i]);
                yield return new WaitUntil(() => gameManager.IsGameOver); 
                EvaluateFitness(newPopulation[i]); 
                SaveGame(newPopulation[i], i + population.Count); 
                yield return new WaitForSeconds(1); 
            }

            population = SelectSurvivors(population, newPopulation);
            WriteChromsToFile(population, filePath2);

            
        }
    }

    List<Chrom> ReadChromsFromFile(string path)
    {
        List<Chrom> Chroms = new List<Chrom>();
        string[] lines = File.ReadAllLines(path);
        Chrom currentChrom = null;

        foreach (string line in lines)
        {
            if (line.StartsWith("Chrom"))
            {
                if (currentChrom != null)
                    Chroms.Add(currentChrom);

                currentChrom = new Chrom();
            }
            else if (currentChrom != null)
            {
                var parts = line.Split(new[] { ':' }, 2);
                if (parts.Length == 2)
                {
                    float value;
                    if (float.TryParse(parts[1].Trim(), out value))
                    {
                        switch (parts[0].Trim())
                        {
                            case "Pacman Punishment Value": currentChrom.PacWomanPunVal = value; break;
                            case "Ghosts Punishment Value": currentChrom.GhostsPunVal = value; break;
                            case "Ghosts Punishment Value 2": currentChrom.GhostsPunVal2 = value; break;
                            case "Pellets Punishment Value": currentChrom.PelletsPunVal = value; break;
                            case "Pellets Too Close Punishment Value": currentChrom.Pellets2ClosePunVal = value; break;
                            case "P Pellets Punishment Value": currentChrom.PPelletsPunVal = value; break;
                            case "P Pellets Too Close Punishment Value": currentChrom.PPellets2ClosePunVal = value; break;
                            case "Fruits Punishment Value": currentChrom.FruitsPunVal = value; break;
                            case "Fruits Too Close Punishment Value": currentChrom.Fruits2ClosePunVal = value; break;
                            case "Fitness": currentChrom.Fitness = (int)value; break;
                        }
                    }
                }
            }
        }
        if (currentChrom != null)
            Chroms.Add(currentChrom);

        return Chroms;
    }

    List<Chrom> SelectParents(List<Chrom> population)
    {
        List<Chrom> matingPool = new List<Chrom>();

        for (int i = 0; i < offspringCount * 2; i++) 
        {
            int tournamentSize = 3; 
            List<Chrom> tournament = new List<Chrom>();
            List<Chrom> tempPopulation = new List<Chrom>(population);

            
            for (int j = 0; j < tournamentSize; j++)
            {
                int index = UnityEngine.Random.Range(0, tempPopulation.Count);
                tournament.Add(tempPopulation[index]);
                tempPopulation.RemoveAt(index); 
            }

            
            Chrom bestChrom = tournament.OrderByDescending(c => c.Fitness).First();
            matingPool.Add(bestChrom);
        }

        return matingPool;
    }


    List<Chrom> SelectSurvivors(List<Chrom> oldPopulation, List<Chrom> newPopulation)
    {
        List<Chrom> combinedPopulation = new List<Chrom>(oldPopulation);
        combinedPopulation.AddRange(newPopulation);
        combinedPopulation = combinedPopulation.OrderByDescending(c => c.Fitness).Take(topChromCount).ToList();
        return combinedPopulation;
    }

    void StartGameWithChrom(Chrom Chrom)
    {
        
        functions.SetCurrentChrom(Chrom);
        gameManager.NewGame(); 
    }

    void EvaluateFitness(Chrom Chrom)
    {
        Chrom.Fitness = lastScore; 
    }

    List<Chrom> SelectParents(List<Chrom> population, int offspringCount)
    {
        List<Chrom> matingPool = new List<Chrom>();

        for (int i = 0; i < offspringCount * 2; i++) 
        {
            int tournamentSize = 3; 
            List<Chrom> tournament = new List<Chrom>();
            List<Chrom> tempPopulation = new List<Chrom>(population);

            for (int j = 0; j < tournamentSize; j++)
            {
                int index = UnityEngine.Random.Range(0, tempPopulation.Count);
                tournament.Add(tempPopulation[index]);
                tempPopulation.RemoveAt(index); 
            }

            Chrom bestChrom = tournament.OrderByDescending(c => c.Fitness).First();
            matingPool.Add(bestChrom);
        }
        return matingPool;
    }


    List<Chrom> Crossover(List<Chrom> crossOverPool)
    {
        List<Chrom> offspring = new List<Chrom>();
        List<Chrom> avbCrossOverPool = new List<Chrom>(crossOverPool);

        
        while (avbCrossOverPool.Count >= 2)
        {
            int index1 = UnityEngine.Random.Range(0, avbCrossOverPool.Count);
            Chrom parent1 = avbCrossOverPool[index1];
            avbCrossOverPool.RemoveAt(index1);

            int index2 = UnityEngine.Random.Range(0, avbCrossOverPool.Count);
            Chrom parent2 = avbCrossOverPool[index2];
            avbCrossOverPool.RemoveAt(index2);

            Debug.Log($" parent1 Pacman Punishment Value: {parent1.PacWomanPunVal}");
            Debug.Log($" parent2 Pacman Punishment Value: {parent2.PacWomanPunVal}");

           
            float alpha = UnityEngine.Random.Range(0f, 1f);
            Debug.Log(alpha);
            Chrom child = new Chrom
            {
                PacWomanPunVal = alpha * parent1.PacWomanPunVal + (1 - alpha) * parent2.PacWomanPunVal,
                GhostsPunVal = alpha * parent1.GhostsPunVal + (1 - alpha) * parent2.GhostsPunVal,
                GhostsPunVal2 = alpha * parent1.GhostsPunVal2 + (1 - alpha) * parent2.GhostsPunVal2,
                PelletsPunVal = alpha * parent1.PelletsPunVal + (1 - alpha) * parent2.PelletsPunVal,
                Pellets2ClosePunVal = alpha * parent1.Pellets2ClosePunVal + (1 - alpha) * parent2.Pellets2ClosePunVal,
                PPelletsPunVal = alpha * parent1.PPelletsPunVal + (1 - alpha) * parent2.PPelletsPunVal,
                PPellets2ClosePunVal = alpha * parent1.PPellets2ClosePunVal + (1 - alpha) * parent2.PPellets2ClosePunVal,
                FruitsPunVal = alpha * parent1.FruitsPunVal + (1 - alpha) * parent2.FruitsPunVal,
                Fruits2ClosePunVal = alpha * parent1.Fruits2ClosePunVal + (1 - alpha) * parent2.Fruits2ClosePunVal
            };

            offspring.Add(child);

            
            Debug.Log($"Offspring: {offspring.Count}:  Pacman Punishment Value: {child.PacWomanPunVal}");
        }

        return offspring;
    }

    List<Chrom> Mutate(List<Chrom> mutationPool, int currentGeneration, int maxGenerations)
    {
        List<Chrom> mutatedPool = new List<Chrom>();

        foreach (var Chrom in mutationPool)
        {
            Chrom mutChrom = new Chrom
            {
                PacWomanPunVal = Chrom.PacWomanPunVal,
                GhostsPunVal = Chrom.GhostsPunVal,
                GhostsPunVal2 = Chrom.GhostsPunVal2,
                PelletsPunVal = Chrom.PelletsPunVal,
                Pellets2ClosePunVal = Chrom.Pellets2ClosePunVal,
                PPelletsPunVal = Chrom.PPelletsPunVal,
                PPellets2ClosePunVal = Chrom.PPellets2ClosePunVal,
                FruitsPunVal = Chrom.FruitsPunVal,
                Fruits2ClosePunVal = Chrom.Fruits2ClosePunVal,
                Fitness = Chrom.Fitness
            };

            float t = (float)currentGeneration / maxGenerations;

            if (UnityEngine.Random.value < mutationRate)
            {
                float delta = UnityEngine.Random.Range(-2500f, 2500f) * (1 - t);
                mutChrom.PacWomanPunVal = Mathf.Clamp(mutChrom.PacWomanPunVal + delta, 500f, 10000f);
            }
            if (UnityEngine.Random.value < mutationRate)
            {
                float delta = UnityEngine.Random.Range(-1000f, 1000f) * (1 - t);
                mutChrom.GhostsPunVal = Mathf.Clamp(mutChrom.GhostsPunVal + delta, 1000f, 5000f);
            }
            if (UnityEngine.Random.value < mutationRate)
            {
                float delta = UnityEngine.Random.Range(-1000f, 1000f) * (1 - t);
                mutChrom.GhostsPunVal2 = Mathf.Clamp(mutChrom.GhostsPunVal2 + delta, 500f, 2000f);
            }
            if (UnityEngine.Random.value < mutationRate)
            {
                float delta = UnityEngine.Random.Range(-150f, 150f) * (1 - t);
                mutChrom.PelletsPunVal = Mathf.Clamp(mutChrom.PelletsPunVal + delta, 50f, 200f);
            }
            if (UnityEngine.Random.value < mutationRate)
            {
                float delta = UnityEngine.Random.Range(-400f, 400f) * (1 - t);
                mutChrom.Pellets2ClosePunVal = Mathf.Clamp(mutChrom.Pellets2ClosePunVal + delta, 500f, 1500f);
            }
            if (UnityEngine.Random.value < mutationRate)
            {
                float delta = UnityEngine.Random.Range(-300f, 300f) * (1 - t);
                mutChrom.PPelletsPunVal = Mathf.Clamp(mutChrom.PPelletsPunVal + delta, 200f, 1000f);
            }
            if (UnityEngine.Random.value < mutationRate)
            {
                float delta = UnityEngine.Random.Range(-400f, 400f) * (1 - t);
                mutChrom.PPellets2ClosePunVal = Mathf.Clamp(mutChrom.PPellets2ClosePunVal + delta, 500f, 1500f);
            }
            if (UnityEngine.Random.value < mutationRate)
            {
                float delta = UnityEngine.Random.Range(-500f, 500f) * (1 - t);
                mutChrom.FruitsPunVal = Mathf.Clamp(mutChrom.FruitsPunVal + delta, 400f, 1000f);
            }
            if (UnityEngine.Random.value < mutationRate)
            {
                float delta = UnityEngine.Random.Range(-1000f, 1000f) * (1 - t);
                mutChrom.Fruits2ClosePunVal = Mathf.Clamp(mutChrom.Fruits2ClosePunVal + delta, 1000f, 2000f);
            }

            mutatedPool.Add(mutChrom);

            
            Debug.Log($"Mutated Chrom: {mutatedPool.Count}:  Pacman Punishment Value: {mutChrom.PacWomanPunVal}");
        }

        return mutatedPool;
    }


    void WriteChromsToFile(List<Chrom> Chroms, string path)
    {
        using (StreamWriter writer = new StreamWriter(path, false))
        {
            for (int i = 0; i < Chroms.Count; i++)
            {
                Chrom Chrom = Chroms[i];
                writer.WriteLine($"Chrom {i + 1}:");
                writer.WriteLine($"  Pacman Punishment Value: {Chrom.PacWomanPunVal}");
                writer.WriteLine($"  Ghosts Punishment Value: {Chrom.GhostsPunVal}");
                writer.WriteLine($"  Ghosts Punishment Value 2: {Chrom.GhostsPunVal2}");
                writer.WriteLine($"  Pellets Punishment Value: {Chrom.PelletsPunVal}");
                writer.WriteLine($"  Pellets Too Close Punishment Value: {Chrom.Pellets2ClosePunVal}");
                writer.WriteLine($"  P Pellets Punishment Value: {Chrom.PPelletsPunVal}");
                writer.WriteLine($"  P Pellets Too Close Punishment Value: {Chrom.PPellets2ClosePunVal}");
                writer.WriteLine($"  Fruits Punishment Value: {Chrom.FruitsPunVal}");
                writer.WriteLine($"  Fruits Too Close Punishment Value: {Chrom.Fruits2ClosePunVal}");
                writer.WriteLine($"  Fitness: {Chrom.Fitness}");
                writer.WriteLine(); // Adds an extra blank line for better readability between entries
            }
        }
    }

    void WriteFitnessToFile(Chrom Chrom, int index)
    {
        string filePath = Path.Combine(desktopPath, fileName);
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine($"Chrom {index + 1}:");
            writer.WriteLine($"  Pacman Punishment Value: {Chrom.PacWomanPunVal}");
            writer.WriteLine($"  Ghosts Punishment Value: {Chrom.GhostsPunVal}");
            writer.WriteLine($"  Ghosts Punishment Value 2: {Chrom.GhostsPunVal2}");
            writer.WriteLine($"  Pellets Punishment Value: {Chrom.PelletsPunVal}");
            writer.WriteLine($"  Pellets Too Close Punishment Value: {Chrom.Pellets2ClosePunVal}");
            writer.WriteLine($"  P Pellets Punishment Value: {Chrom.PPelletsPunVal}");
            writer.WriteLine($"  P Pellets Too Close Punishment Value: {Chrom.PPellets2ClosePunVal}");
            writer.WriteLine($"  Fruits Punishment Value: {Chrom.FruitsPunVal}");
            writer.WriteLine($"  Fruits Too Close Punishment Value: {Chrom.Fruits2ClosePunVal}");
            writer.WriteLine($"  Fitness: {Chrom.Fitness}");
            writer.WriteLine(); // Adds an extra blank line for better readability between entries
        }
    }

    
    void SaveGame(Chrom Chrom, int index)
    {
        EvaluateFitness(Chrom);
        WriteFitnessToFile(Chrom, index);
    }
}
