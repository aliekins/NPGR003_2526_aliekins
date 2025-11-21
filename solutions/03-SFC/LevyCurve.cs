namespace _03_SFC
{
    internal sealed class LevyCurve : ICurve
    {
        public string Name => "levy";
        public string Description => "Lévy C-curve (recursive segment replacement)";
        public bool IsSpaceFilling => false;
        public bool IsImplemented => true;

        public List<Vec2> Generate(int depth)
        {
            if (depth < 0) depth = 0;
            if (depth > 17) depth = 17; // 2^17 segments

            List<Vec2> pts = new List<Vec2>();
            Vec2 a = new Vec2(0.0, 0.0);
            Vec2 b = new Vec2(1.0, 0.0);
            LevyRecursive(a, b, depth, pts);
            return pts;
        }

        private static void LevyRecursive(Vec2 a, Vec2 b, int depth, List<Vec2> pts)
        {
            if (depth == 0)
            {
                if (pts.Count == 0)
                    pts.Add(a);
                pts.Add(b);
                return;
            }

            Vec2 m = MidLevy(a, b);
            LevyRecursive(a, m, depth - 1, pts);
            LevyRecursive(m, b, depth - 1, pts);
        }

        private static Vec2 MidLevy(Vec2 a, Vec2 b)
        {
            // midpoint rotated by +45 in the complex plane
            double mx = (a.X + b.X) / 2.0 - (b.Y - a.Y) / 2.0;
            double my = (a.Y + b.Y) / 2.0 + (b.X - a.X) / 2.0;
            return new Vec2(mx, my);
        }
    }
}