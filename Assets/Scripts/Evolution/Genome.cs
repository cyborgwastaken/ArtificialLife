namespace ArtificialLife
{
    // TEMPORARY STUB - replaced in Milestone 4.
    public sealed class Genome
    {
        public int[] LayerSizes;
        public float[] Genes;
        public int Id, ParentId, GenerationBorn;

        public Genome(int[] layerSizes, float[] genes) { LayerSizes = layerSizes; Genes = genes; }

        public static Genome Random(int[] layerSizes, Rng rng, int id, int generation)
        {
            var net = new NeuralNetwork(layerSizes);
            net.Randomize(rng);
            var genes = new float[net.ParameterCount];
            net.WriteParameters(genes);
            return new Genome(layerSizes, genes) { Id = id, ParentId = -1, GenerationBorn = generation };
        }

        public NeuralNetwork BuildNetwork()
        {
            var net = new NeuralNetwork(LayerSizes);
            net.ReadParameters(Genes);
            return net;
        }
    }
}
