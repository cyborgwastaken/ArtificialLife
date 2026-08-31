using System.Collections.Generic;
using UnityEngine;

namespace ArtificialLife
{
    public readonly struct Candidate
    {
        public readonly Genome Genome;
        public readonly float Fitness;
        public Candidate(Genome g, float f) { Genome = g; Fitness = f; }
    }

    public static class Selection
    {
        /// Best of k random picks. k=1 -> random (no pressure). k=pool.Count -> always the best
        /// (max pressure, kills diversity). k=3 is a good default.
        public static Genome Tournament(List<Candidate> pool, Rng rng, int k)
        {
            Candidate best = pool[rng.RangeInt(0, pool.Count)];
            for (int n = 1; n < k; n++)
            {
                Candidate c = pool[rng.RangeInt(0, pool.Count)];
                if (c.Fitness > best.Fitness) best = c;
            }
            return best.Genome;
        }

        /// Fitness-proportionate ("roulette wheel"). Probability of a genome ∝ its fitness.
        /// Assumes fitness >= 0. Provided for comparison; tournament is the default.
        public static Genome Roulette(List<Candidate> pool, Rng rng)
        {
            float total = 0f;
            for (int i = 0; i < pool.Count; i++) total += Mathf.Max(0f, pool[i].Fitness);
            if (total <= 0f) return pool[rng.RangeInt(0, pool.Count)].Genome;

            float r = rng.NextFloat() * total;    // spin the wheel
            float acc = 0f;
            for (int i = 0; i < pool.Count; i++)
            {
                acc += Mathf.Max(0f, pool[i].Fitness);
                if (acc >= r) return pool[i].Genome;
            }
            return pool[pool.Count - 1].Genome;
        }

        /// The 'count' highest-fitness genomes, best first.
        public static List<Genome> TopElites(List<Candidate> pool, int count)
        {
            var sorted = new List<Candidate>(pool);
            sorted.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));
            var result = new List<Genome>();
            for (int i = 0; i < count && i < sorted.Count; i++)
                result.Add(sorted[i].Genome);
            return result;
        }
    }
}
