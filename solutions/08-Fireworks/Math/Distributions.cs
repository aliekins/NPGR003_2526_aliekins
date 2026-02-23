namespace _08_Fireworks.Math
{
    public static class Distributions
    {
        // Bernoulli event
        public static bool Chance (IRandSource rng, double p)
        {
            if (p <= 0.0)
            {
                return false;
            }

            if (p >= 1.0)
            {
                return true;
            }

            return rng.Next01() < p;
        }

        // Poisson like rate per sec event for Distributions
        public static bool EventFromRate (IRandSource rng, double ratePerSec, double dt)
        {
            if (ratePerSec <= 0.0 || dt <= 0.0)
            {
                return false;
            }

            var p = 1.0 - System.Math.Exp(-ratePerSec * dt);
            return rng.Next01() < p;
        }
    }
}