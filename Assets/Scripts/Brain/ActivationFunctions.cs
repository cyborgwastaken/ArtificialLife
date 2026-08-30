using System;

namespace ArtificialLife
{
    public enum Activation { Tanh, Sigmoid, ReLU, Linear }

    public static class ActivationFunctions
    {
        public static float Apply(Activation a, float x)
        {
            switch (a)
            {
                case Activation.Tanh:    return MathF.Tanh(x);
                case Activation.Sigmoid: return 1f / (1f + MathF.Exp(-x));
                case Activation.ReLU:    return x > 0f ? x : 0f;
                case Activation.Linear:  return x;
                default:                 return x;
            }
        }
        // If your Unity build lacks System.MathF, use (float)Math.Tanh(x) and (float)Math.Exp(-x).
    }
}
