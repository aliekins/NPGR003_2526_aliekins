using System;
using System.Collections.Generic;

namespace _03_SFC
{
    internal sealed class BarnsleyFernCurve : ICurve
    {
        public string Name => "barnsley-fern";
        public string Description =>
            "Barnsley fern (IFS, random iteration point cloud)";
        public bool IsSpaceFilling => false;
        public bool IsImplemented => true;

        public List<Vec2> Generate(int depth)
        {
            if (depth < 0) depth = 0;
            if (depth > 10) depth = 10;

            int iterations = 20000 + depth * 20000;
            if (iterations > 250000)
                iterations = 250000;

            var pts = new List<Vec2>(iterations);

            double x = 0.0;
            double y = 0.0;
            var rnd = new Random(123456);

            for (int i = 0; i < 50; i++)
            {
                ApplyBarnsleyStep(rnd.NextDouble(), ref x, ref y);
            }

            for (int i = 0; i < iterations; i++)
            {
                ApplyBarnsleyStep(rnd.NextDouble(), ref x, ref y);
                pts.Add(new Vec2(x, y));
            }

            if (pts.Count < 2)
            {
                pts.Clear();
                pts.Add(new Vec2(0.0, 0.0));
                pts.Add(new Vec2(1.0, 1.0));
            }

            return pts;
        }

        private static void ApplyBarnsleyStep(double r, ref double x, ref double y)
        {
            double nx, ny;

            if (r < 0.01)
            {
                nx = 0.0;
                ny = 0.16 * y;
            }
            else if (r < 0.86)
            {
                nx = 0.85 * x + 0.04 * y;
                ny = -0.04 * x + 0.85 * y + 1.6;
            }
            else if (r < 0.93)
            {
                nx = 0.20 * x - 0.26 * y;
                ny = 0.23 * x + 0.22 * y + 1.6;
            }
            else
            {
                nx = -0.15 * x + 0.28 * y;
                ny = 0.26 * x + 0.24 * y + 0.44;
            }

            x = nx;
            y = ny;
        }
    }
}
