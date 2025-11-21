namespace _03_SFC
{
    internal sealed class HilbertCurve : ICurve
    {
        public string Name => "hilbert";
        public string Description => "Hilbert space-filling curve (recursive parametric)";
        public bool IsSpaceFilling => true;
        public bool IsImplemented => true;

        public List<Vec2> Generate(int depth)
        {
            if (depth < 0) depth = 0;
            if (depth > 9) depth = 9;

            List<Vec2> pts = new List<Vec2>();
            HilbertRecursive(depth, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0, pts);
            return pts;
        }

        private static void HilbertRecursive(
          int level,
          double x, double y,
          double xi, double xj,
          double yi, double yj,
          List<Vec2> pts)
        {
            if (level <= 0)
            {
                double px = x + (xi + yi) / 2.0;
                double py = y + (xj + yj) / 2.0;
                pts.Add(new Vec2(px, py));
            }
            else
            {
                level--;
                double xi2 = xi / 2.0;
                double xj2 = xj / 2.0;
                double yi2 = yi / 2.0;
                double yj2 = yj / 2.0;

                HilbertRecursive(level, x, y, yi2, yj2, xi2, xj2, pts);
                HilbertRecursive(level, x + xi2, y + xj2, xi2, xj2, yi2, yj2, pts);
                HilbertRecursive(level, x + xi2 + yi2, y + xj2 + yj2, xi2, xj2, yi2, yj2, pts);
                HilbertRecursive(level, x + xi2 + yi, y + xj2 + yj, -yi2, -yj2, -xi2, -xj2, pts);
            }
        }
    }
}