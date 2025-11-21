using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace _03_SFC
{
    internal sealed class SierpinskiCurve : ICurve
    {
        public string Name => "sierpinski";
        public string Description =>
          "Sierpiński arrowhead fractal curve (L-system, triangular).";
        public bool IsSpaceFilling => false; // not space-filling, just fractal
        public bool IsImplemented => true;

        public List<Vec2> Generate(int depth)
        {
            if (depth < 0) depth = 0;
            if (depth > 9) depth = 9;

            string seq = "A";
            for (int i = 0; i < depth; i++)
            {
                StringBuilder next = new StringBuilder(seq.Length * 3);
                foreach (char c in seq)
                {
                    switch (c)
                    {
                        case 'A':
                            next.Append("B-A-B");
                            break;
                        case 'B':
                            next.Append("A+B+A");
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
                    // turn left
                    angle += rad60;
                }
                else if (c == '-')
                {
                    // turn right
                    angle -= rad60;
                }
            }

            return pts;
        }
    }
}