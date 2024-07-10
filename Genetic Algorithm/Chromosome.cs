using System;


public class Chrom
{
    public float PacWomanPunVal;
    public float GhostsPunVal;
    public float GhostsPunVal2;
    public float PelletsPunVal;
    public float Pellets2ClosePunVal;
    public float PPelletsPunVal;
    public float PPellets2ClosePunVal;
    public float FruitsPunVal;
    public float Fruits2ClosePunVal;
    public float Fitness;  // Fitness value of the Chrom

    private Random random = new Random();
    // Constructor to initialize a random Chrom
    public Chrom()
    {
        PacWomanPunVal = (float)(random.NextDouble() * (10000f - 500f) + 500f);
        GhostsPunVal = (float)(random.NextDouble() * (5000f - 1000f) + 1000f);
        GhostsPunVal2 = (float)(random.NextDouble() * (2000f - 500f) + 500f);
        PelletsPunVal = (float)(random.NextDouble() * (200f - 50f) + 50f);
        Pellets2ClosePunVal = (float)(random.NextDouble() * (1500f - 500f) + 500f);
        PPelletsPunVal = (float)(random.NextDouble() * (1000f - 200f) + 200f);
        PPellets2ClosePunVal = (float)(random.NextDouble() * (1500f - 500f) + 500f);
        FruitsPunVal = (float)(random.NextDouble() * (1000f - 400f) + 400f);
        Fruits2ClosePunVal = (float)(random.NextDouble() * (2000f - 1000f) + 1000f);
    }

    // Copy constructor
    public Chrom(Chrom other)
    {
        PacWomanPunVal = other.PacWomanPunVal;
        GhostsPunVal = other.GhostsPunVal;
        GhostsPunVal2 = other.GhostsPunVal2;
        PelletsPunVal = other.PelletsPunVal;
        Pellets2ClosePunVal = other.Pellets2ClosePunVal;
        PPelletsPunVal = other.PPelletsPunVal;
        PPellets2ClosePunVal = other.PPellets2ClosePunVal;
        FruitsPunVal = other.FruitsPunVal;
        Fruits2ClosePunVal = other.Fruits2ClosePunVal;
        Fitness = other.Fitness;
    }
}
