namespace _04Mandala.Core
{
    public static class MathExtensions
    {
        public static float Clamp (float v, float min, float max)
        {
            if (v < min)
                return min;
            if (v > max)
                return max;
            return v;
        }

        public static float Clamp01 (float v)
        {
            if (v < 0f)
                return 0f;
            if (v > 1f)
                return 1f;
            return v;
        }

        public static float Hash (int x, int y, int seed)
        {
            unchecked
            {
                int h = seed;
                h = h * 31 + x;
                h = h * 31 + y;
                h ^= (h >> 13);
                h *= 0x5bd1e995;
                h ^= (h >> 15);

                uint u = (uint)h;
                return (u & 0xFFFFFF) / (float)0x1000000;
            }
        }

        public static float Wrap (float v)
        {
            v = v - (float)Math.Floor(v);
            if (v < 0f)
                v += 1f;
            return v;
        }

    }
}