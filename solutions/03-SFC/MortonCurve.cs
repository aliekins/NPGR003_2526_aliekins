namespace _03_SFC
{
    internal sealed class MortonCurve : ICurve
    {
        public string Name => "morton";
        public string Description => "Morton / Z-order curve (bit-interleaving SFC)";
        public bool IsSpaceFilling => true;
        public bool IsImplemented => true;

        public List<Vec2> Generate(int depth)
        {
            if (depth < 1) depth = 1;
            if (depth > 10) depth = 10; // 2^(2*10) = 1,048,576 points

            int n = 1 << depth;
            int count = n * n;
            List<Vec2> pts = new List<Vec2>(count);

            for (int index = 0; index < count; index++)
            {
                int x = 0;
                int y = 0;
                for (int bit = 0; bit < depth; bit++)
                {
                    int xb = (index >> (2 * bit)) & 1;
                    int yb = (index >> (2 * bit + 1)) & 1;
                    x |= xb << bit;
                    y |= yb << bit;
                }

                double fx = (x + 0.5) / n;
                double fy = (y + 0.5) / n;
                pts.Add(new Vec2(fx, fy));
            }

            return pts;
        }
    }
}