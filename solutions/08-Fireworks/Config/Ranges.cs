namespace _08_Fireworks.Config
{
    public readonly struct RangeDouble
    {
        public readonly double Min;
        public readonly double Max;

        public RangeDouble (double min, double max)
        {
            Min = min;
            Max = max;
        }

        public double Clamp (double x)
        {
            if (x < Min)
                return Min;
            if (x > Max)
                return Max;
            return x;
        }
    }

    public readonly struct RangeInt
    {
        public readonly int Min;
        public readonly int Max;

        public RangeInt (int min, int max)
        {
            Min = min;
            Max = max;
        }
    }
}