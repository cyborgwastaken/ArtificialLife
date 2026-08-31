using System;

namespace ArtificialLife
{
    [Serializable]
    public sealed class Genome
    {
        public int[] LayerSizes;
        public float[] Genes;          // weights + biases; order = NeuralNetwork.WriteParameters

        public int Id = -1;
        public int ParentId = -1;
        public int GenerationBorn;

        public Genome(int[] layerSizes, float[] genes)
        {
            LayerSizes = (int[])layerSizes.Clone();
            Genes = (float[])genes.Clone();
        }

        public static Genome Random(int[] layerSizes, Rng rng, int id, int generation)
        {
            var net = new NeuralNetwork(layerSizes);
            net.Randomize(rng);
            var genes = new float[net.ParameterCount];
            net.WriteParameters(genes);
            return new Genome(layerSizes, genes)
            {
                Id = id, ParentId = -1, GenerationBorn = generation
            };
        }

        public NeuralNetwork BuildNetwork(Activation hidden = Activation.Tanh,
                                          Activation output = Activation.Tanh)
        {
            var net = new NeuralNetwork(LayerSizes, hidden, output);
            net.ReadParameters(Genes);
            return net;
        }

        /// Deep copy including genes; keeps lineage fields (caller overwrites Id etc. as needed).
        public Genome Clone() => new Genome(LayerSizes, Genes)
        {
            Id = Id, ParentId = ParentId, GenerationBorn = GenerationBorn
        };
    }
}
