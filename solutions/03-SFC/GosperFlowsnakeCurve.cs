using System;
using System.Text;

namespace _03_SFC
{
    internal sealed class GosperFlowsnakeCurve : ICurve
    {
        public string Name => "gosper";
        public string Description => "Gosper flowsnake (hexagonal L-system fractal)";
        public bool IsSpaceFilling => false;
        public bool IsImplemented => true;

        public List<Vec2> Generate(int depth)
        {
            if (depth < 0) depth = 0;
            if (depth > 5) depth = 5;

            string seq = "A";
            for (int i = 0; i < depth; i++)
            {
                StringBuilder next = new StringBuilder(seq.Length * 7);
                foreach (char c in seq)
                {
                    switch (c)
                    {
                        case 'A':
                            next.Append("A+B++B-A--AA-B+");
                            break;
                        case 'B':
                            next.Append("-A+BB++B+A--A-B");
                            break;
                        default:
                            next.Append(c);
                            break;
                    }
                }
                seq = next.ToString();
            }

            List<Vec2> pts = new List<Vec2>();
            double x = 0.0;
            double y = 0.0;
            double angle = 0.0;
            double step = 1.0;
            double rad60 = Math.PI / 3.0;

            pts.Add(new Vec2(x, y));

            foreach (char c in seq)
            {
                if (c == 'A' || c == 'B')
                {
                    x += Math.Cos(angle) * step;
                    y += Math.Sin(angle) * step;
                    pts.Add(new Vec2(x, y));
                }
                else if (c == '+')
                {
                    angle += rad60;
                }
                else if (c == '-')
                {
                    angle -= rad60;
                }
            }

            return pts;
        }
    }
}