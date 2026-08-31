using System;
using System.Collections.Generic;

namespace ArtificialLife
{
    [Serializable]
    public sealed class PopulationSave
    {
        public int version = 1;
        public int seed;
        public int generation;
        public int nextGenomeId;
        public List<Genome> seedGenomes = new List<Genome>();
        public List<GenerationStats> history = new List<GenerationStats>();
    }
}
