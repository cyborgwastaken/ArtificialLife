using System;

namespace ArtificialLife
{
    /// Fixed-topology feed-forward net. Prototype topology: 8 -> 6 -> 3, tanh throughout.
    /// Pure C#: no MonoBehaviour, no UnityEngine dependency - so it is unit-testable in isolation.
    public sealed class NeuralNetwork
    {
        public readonly int[] LayerSizes;
        public readonly Layer[] Layers;

        public int InputSize  => LayerSizes[0];
        public int OutputSize => LayerSizes[LayerSizes.Length - 1];

        public NeuralNetwork(int[] layerSizes,
                             Activation hiddenActivation = Activation.Tanh,
                             Activation outputActivation = Activation.Tanh)
        {
            if (layerSizes == null || layerSizes.Length < 2)
                throw new ArgumentException("Need at least an input and an output layer.");

            LayerSizes = (int[])layerSizes.Clone();
            Layers = new Layer[layerSizes.Length - 1];
            for (int l = 0; l < Layers.Length; l++)
            {
                bool isOutput = l == Layers.Length - 1;
                Layers[l] = new Layer(layerSizes[l], layerSizes[l + 1],
                                      isOutput ? outputActivation : hiddenActivation);
            }
        }

        /// input.Length must equal InputSize. Returns a fresh array of length OutputSize.
        public float[] FeedForward(float[] input)
        {
            float[] signal = input;
            for (int l = 0; l < Layers.Length; l++)
                signal = Layers[l].Forward(signal);

            var result = new float[signal.Length];
            Array.Copy(signal, result, signal.Length);
            return result;
        }

        /// weights + biases across all layers.
        public int ParameterCount
        {
            get
            {
                int n = 0;
                foreach (var layer in Layers)
                    n += layer.OutputSize * layer.InputSize + layer.OutputSize;
                return n;
            }
        }

        /// Fixed serialisation order: for each layer -> all weights row-major (neuron by neuron),
        /// then all biases. WriteParameters and ReadParameters MUST agree on this order.
        public void WriteParameters(float[] genes)
        {
            Require(genes);
            int k = 0;
            foreach (var layer in Layers)
            {
                for (int o = 0; o < layer.OutputSize; o++)
                    for (int i = 0; i < layer.InputSize; i++)
                        genes[k++] = layer.Weights[o, i];
                for (int o = 0; o < layer.OutputSize; o++)
                    genes[k++] = layer.Biases[o];
            }
        }

        public void ReadParameters(float[] genes)
        {
            Require(genes);
            int k = 0;
            foreach (var layer in Layers)
            {
                for (int o = 0; o < layer.OutputSize; o++)
                    for (int i = 0; i < layer.InputSize; i++)
                        layer.Weights[o, i] = genes[k++];
                for (int o = 0; o < layer.OutputSize; o++)
                    layer.Biases[o] = genes[k++];
            }
        }

        void Require(float[] genes)
        {
            if (genes.Length != ParameterCount)
                throw new ArgumentException($"Expected {ParameterCount} genes, got {genes.Length}.");
        }

        /// Random N(0, sigma) init. See guide section 3.6 for why these scales.
        public void Randomize(Rng rng, float weightStdDev = 0.5f, float biasStdDev = 0.1f)
        {
            foreach (var layer in Layers)
                for (int o = 0; o < layer.OutputSize; o++)
                {
                    for (int i = 0; i < layer.InputSize; i++)
                        layer.Weights[o, i] = rng.NextGaussian(0f, weightStdDev);
                    layer.Biases[o] = rng.NextGaussian(0f, biasStdDev);
                }
        }
    }
}
