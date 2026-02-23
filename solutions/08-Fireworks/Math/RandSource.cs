using System;

namespace _08_Fireworks.Math
{
    public sealed class RandSource : IRandSource
    {
        private readonly Random _rnd;
        public int Seed { get; }

        public RandSource (int seed)
        {
            Seed = seed;
            _rnd = new Random(seed);
        }

        public double Next01 ()
        {
            return _rnd.NextDouble();
        }

        public double Next (double min, double max)
        {
            return min + (max - min) * _rnd.NextDouble();
        }

        public int NextInt (int minInc, int maxInc)
        {
            if (maxInc < minInc)
            {
                return minInc;
            }

            return _rnd.Next(minInc, maxInc + 1);
        }
    }
}