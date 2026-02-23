namespace _08_Fireworks.Physics
{
    public static class PhysicsUtil
    {
        public static double ClampDt (double dt, double maxDt)
        {
            if (dt < 0.0)
            {
                return 0.0;
            }
            if (dt > maxDt)
            {
                return maxDt;
            }

            return dt;
        }
    }
}