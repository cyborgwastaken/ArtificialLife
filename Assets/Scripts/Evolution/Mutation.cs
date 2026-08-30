namespace ArtificialLife
{
    public static class Mutation
    {
        /// Perturb 'genes' in place.
        public static void MutateInPlace(float[] genes, Rng rng, float rate, float strength,
                                         float resetChance = 0.05f, float resetStdDev = 0.5f,
                                         float clamp = 8f)
        {
            for (int i = 0; i < genes.Length; i++)
            {
                if (rng.NextFloat() >= rate) continue;          // this gene untouched

                if (rng.NextFloat() < resetChance)
                    genes[i] = rng.NextGaussian(0f, resetStdDev);   // rare big jump
                else
                    genes[i] += rng.NextGaussian(0f, strength);     // usual small nudge

                if (genes[i] >  clamp) genes[i] =  clamp;
                if (genes[i] < -clamp) genes[i] = -clamp;
            }
        }

        /// Clone 'parent', mutate the copy, stamp lineage fields.
        public static Genome Mutated(Genome parent, Rng rng, float rate, float strength,
                                     int childId, int generation)
        {
            var child = new Genome(parent.LayerSizes, parent.Genes);
            MutateInPlace(child.Genes, rng, rate, strength);
            child.Id = childId;
            child.ParentId = parent.Id;
            child.GenerationBorn = generation;
            return child;
        }
    }
}
