namespace _08_Fireworks.Launchers
{
    public static class SpawnThrottle
    {
        public static double Compute (double fillRatio, double start, double hard)
        {
            if (fillRatio <= start)
            {
                return 1.0;
            }
            if (fillRatio >= hard)
            {
                return 0.0;
            }

            // linear ramp down from start
            var t = (fillRatio - start) / (hard - start);
            return 1.0 - t;
        }
    }
}