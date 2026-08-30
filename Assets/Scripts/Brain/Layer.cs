using System;

namespace ArtificialLife
{
    /// One fully-connected layer: output = activation(Weights * input + Biases).
    /// Weights[o, i] is the weight from input i into output neuron o.
    public sealed class Layer
    {
        public readonly int InputSize;
        public readonly int OutputSize;
        public readonly float[,] Weights;   // [OutputSize, InputSize]
        public readonly float[] Biases;     // [OutputSize]
        public readonly Activation Activation;

        // Reused every call so a tick allocates nothing.
        readonly float[] _output;

        public Layer(int inputSize, int outputSize, Activation activation)
        {
            InputSize = inputSize;
            OutputSize = outputSize;
            Activation = activation;
            Weights = new float[outputSize, inputSize];
            Biases = new float[outputSize];
            _output = new float[outputSize];
        }

        /// Returns an internal buffer - copy it if you need to keep it past the next call.
        public float[] Forward(float[] input)
        {
            if (input.Length != InputSize)
                throw new ArgumentException($"Layer expected {InputSize} inputs, got {input.Length}.");

            for (int o = 0; o < OutputSize; o++)
            {
                float sum = Biases[o];
                for (int i = 0; i < InputSize; i++)
                    sum += Weights[o, i] * input[i];
                _output[o] = ActivationFunctions.Apply(Activation, sum);
            }
            return _output;
        }
    }
}
