namespace ArtificialLife
{
    [System.Serializable]
    public struct GenerationStats
    {
        public int Generation;
        public int Cohort;              // organisms that lived this generation
        public float AvgLifespan;
        public float MaxLifespan;
        public float AvgEnergyAtEnd;
        public int TotalFoodConsumed;
        public int TotalOffspring;
        public float AvgFitness;
        public float MaxFitness;
    }
}
