using System;

namespace ArtificialLife
{
    /// Deterministic random source. Create ONE per simulation from the seed and thread it
    /// through everything. Never call UnityEngine.Random anywhere in the sim - it is global
    /// mutable state and will silently break seed reproducibility.
    public sealed class Rng
    {
        readonly System.Random _r;
        double _spareGaussian;
        bool _hasSpare;

        public Rng(int seed) => _r = new System.Random(seed);

        /// Uniform in [0, 1).
        public float NextFloat() => (float)_r.NextDouble();

        /// Uniform in [min, max).
        public float Range(float min, float max) => min + (float)_r.NextDouble() * (max - min);

        /// Uniform integer in [minInclusive, maxExclusive).
        public int RangeInt(int minInclusive, int maxExclusive) => _r.Next(minInclusive, maxExclusive);

        /// Standard normal N(0, 1) via the polar (Marsaglia) form of Box-Muller.
        /// It produces two independent normals per pass; we cache the spare.
        public float NextGaussian()
        {
            if (_hasSpare) { _hasSpare = false; return (float)_spareGaussian; }

            double u, v, s;
            do
            {
                u = _r.NextDouble() * 2.0 - 1.0;   // uniform in (-1, 1)
                v = _r.NextDouble() * 2.0 - 1.0;
                s = u * u + v * v;                  // point inside the unit disc?
            } while (s >= 1.0 || s == 0.0);

            double mul = Math.Sqrt(-2.0 * Math.Log(s) / s);
            _spareGaussian = v * mul;
            _hasSpare = true;
            return (float)(u * mul);
        }

        /// Normal with the given mean and standard deviation.
        public float NextGaussian(float mean, float stdDev) => mean + NextGaussian() * stdDev;
    }
}
