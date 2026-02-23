namespace _08_Fireworks.Particles
{
    public readonly struct PoolStats
    {
        public readonly int Alive;
        public readonly int Capacity;

        public PoolStats (int alive, int capacity)
        {
            Alive = alive;
            Capacity = capacity;
        }

        public double FillRatio => Capacity == 0 ? 0.0 : (double)Alive / Capacity;
    }
}