using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace _03_SFC
{
    internal sealed class PeanoCurve : ICurve
    {
        public string Name => "peano";
        public string Description =>
          "Peano space-filling curve (3x3 grid via L-system).";
        public bool IsSpaceFilling => true;
        public bool IsImplemented => true;

        public List<Vec2> Generate(int depth)
        {
            if (depth < 0) depth = 0;
            if (depth > 5) depth = 5;

            string seq = "L";
            for (int i = 0; i < depth; i++)
            {
                StringBuilder next = new StringBuilder(seq.Length * 9);
                foreach (char c in seq)
                {
                    switch (c)
                    {
                        case 'L':
                            next.Append("LFRFL-F-RFLFR+F+LFRFL");
                            break;
                        case 'R':
                            next.Append("RFLFR+F+LFRFL-F-RFLFR");
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
            double rad90 = Math.PI / 2.0;

            pts.Add(new Vec2(x, y));

            foreach (char c in seq)
            {
                switch (c)
                {
                    case 'F':
                        x += Math.Cos(angle) * step;
                        y += Math.Sin(angle) * step;
                        pts.Add(new Vec2(x, y));
                        break;
                    case '+':
                        angle += rad90;
                        break;
                    case '-':
                        angle -= rad90;
                        break;
                }
            }

            return pts;
        }
    }
}