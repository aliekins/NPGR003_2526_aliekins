namespace _03_SFC
{
    internal sealed class NewtonFractalCurve : ICurve
    {
        public string Name => "newton";
        public string Description =>
          "Newton fractal (z^3 - 1 basins boundary approximation via polyline).";
        public bool IsSpaceFilling => false;
        public bool IsImplemented => true;

        public List<Vec2> Generate(int depth)
        {
            if (depth < 1) depth = 1;
            if (depth > 7) depth = 7;

            int n = 32 * depth;

            int[,] roots = new int[n + 1, n + 1];

            for (int j = 0; j <= n; j++)
            {
                double imag = 3.0 * (j / (double)n - 0.5);
                for (int i = 0; i <= n; i++)
                {
                    double real = 3.0 * (i / (double)n - 0.5);
                    roots[i, j] = ClassifyRoot(real, imag);
                }
            }

            List<Vec2> pts = new List<Vec2>();

            for (int j = 0; j <= n; j++)
            {
                for (int i = 0; i < n; i++)
                {
                    int r1 = roots[i, j];
                    int r2 = roots[i + 1, j];
                    if (r1 != r2)
                    {
                        double x1 = i / (double)n;
                        double y1 = j / (double)n;
                        double x2 = (i + 1) / (double)n;
                        double y2 = y1;

                        pts.Add(new Vec2(x1, y1));
                        pts.Add(new Vec2(x2, y2));
                    }
                }
            }

            for (int j = 0; j < n; j++)
            {
                for (int i = 0; i <= n; i++)
                {
                    int r1 = roots[i, j];
                    int r2 = roots[i, j + 1];
                    if (r1 != r2)
                    {
                        double x1 = i / (double)n;
                        double y1 = j / (double)n;
                        double x2 = x1;
                        double y2 = (j + 1) / (double)n;

                        pts.Add(new Vec2(x1, y1));
                        pts.Add(new Vec2(x2, y2));
                    }
                }
            }

            if (pts.Count < 2)
            {
                pts.Clear();
                pts.Add(new Vec2(0.0, 0.0));
                pts.Add(new Vec2(1.0, 1.0));
            }

            return pts;
        }

        private static int ClassifyRoot(double real, double imag)
        {
            double zr = real;
            double zi = imag;

            const int maxIter = 40;
            const double tol2 = 1e-8;

            for (int k = 0; k < maxIter; k++)
            {
                double zr2 = zr * zr - zi * zi;
                double zi2 = 2.0 * zr * zi;

                double fr = zr * zr2 - zi * zi2 - 1.0;
                double fi = zr * zi2 + zi * zr2;

                double fm2 = fr * fr + fi * fi;
                if (fm2 < tol2)
                {
                    return NearestRoot(zr, zi);
                }

                double dfr = 3.0 * zr2;
                double dfi = 3.0 * zi2;
                double denom = dfr * dfr + dfi * dfi;
                if (denom == 0.0)
                    break;

                double rr = (fr * dfr + fi * dfi) / denom;
                double ri = (fi * dfr - fr * dfi) / denom;

                zr -= rr;
                zi -= ri;
            }

            return NearestRoot(zr, zi, -1);
        }

        private static int NearestRoot(double zr, double zi, int defaultIfFar = 0)
        {
            const double r0r = 1.0;
            const double r0i = 0.0;

            const double r1r = -0.5;
            const double r1i = 0.8660254037844386;  // sqrt(3)/2

            const double r2r = -0.5;
            const double r2i = -0.8660254037844386;

            double d0 = (zr - r0r) * (zr - r0r) + (zi - r0i) * (zi - r0i);
            double d1 = (zr - r1r) * (zr - r1r) + (zi - r1i) * (zi - r1i);
            double d2 = (zr - r2r) * (zr - r2r) + (zi - r2i) * (zi - r2i);

            double min = d0;
            int idx = 0;

            if (d1 < min)
            {
                min = d1;
                idx = 1;
            }
            if (d2 < min)
            {
                min = d2;
                idx = 2;
            }

            if (defaultIfFar != 0 && min > 0.25)
                return defaultIfFar;

            return idx;
        }
    }
}