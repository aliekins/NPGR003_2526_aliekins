namespace _03_SFC
{
    internal sealed class DragonCurve : ICurve
    {
        public string Name => "dragon";
        public string Description => "Heighway dragon fractal curve (L-system style turns)";
        public bool IsSpaceFilling => false;
        public bool IsImplemented => true;

        public List<Vec2> Generate(int depth)
        {
            if (depth < 0) depth = 0;
            if (depth > 18) depth = 18; // 2^18 segments

            string turns = GenerateTurnSequence(depth);
            List<Vec2> pts = new List<Vec2>(turns.Length + 1);

            // start at origin, heading right
            Vec2 current = new Vec2(0.0, 0.0);
            int dx = 1;
            int dy = 0;
            pts.Add(current);

            for (int i = 0; i < turns.Length; i++)
            {
                char t = turns[i];
                if (t == 'L')
                {
                    // rotate +90
                    int ndx = -dy;
                    int ndy = dx;
                    dx = ndx;
                    dy = ndy;
                }
                else if (t == 'R')
                {
                    // rotate -90
                    int ndx = dy;
                    int ndy = -dx;
                    dx = ndx;
                    dy = ndy;
                }

                current = new Vec2(current.X + dx, current.Y + dy);
                pts.Add(current);
            }

            // normalize to roughly [0,1] x [0,1]
            return Normalize(pts);
        }

        private static string GenerateTurnSequence(int depth)
        {
            if (depth == 0)
                return "";

            string seq = "L";
            for (int i = 2; i <= depth; i++)
            {
                char[] rev = new char[seq.Length];
                for (int j = 0; j < seq.Length; j++)
                {
                    char c = seq[seq.Length - 1 - j];
                    rev[j] = (c == 'L') ? 'R' : 'L';
                }
                seq = seq + "L" + new string(rev);
            }
            return seq;
        }

        private static List<Vec2> Normalize(List<Vec2> pts)
        {
            double minX = double.PositiveInfinity;
            double minY = double.PositiveInfinity;
            double maxX = double.NegativeInfinity;
            double maxY = double.NegativeInfinity;

            foreach (var p in pts)
            {
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
            }

            double dx = maxX - minX;
            double dy = maxY - minY;
            double scale = (dx > dy) ? 1.0 / (dx == 0 ? 1.0 : dx) : 1.0 / (dy == 0 ? 1.0 : dy);

            List<Vec2> res = new List<Vec2>(pts.Count);
            for (int i = 0; i < pts.Count; i++)
            {
                var p = pts[i];
                double x = (p.X - minX) * scale;
                double y = (p.Y - minY) * scale;
                res.Add(new Vec2(x, y));
            }
            return res;
        }
    }
}