namespace _08_Fireworks.Effects
{
    public static class StrobeModel
    {
        // brightness in [0,1]
        public static float Evaluate (double timeSeconds, float hz, float phase01, double duty01, bool hard)
        {
            if (hz <= 1e-6f)
            {
                return 1.0f;
            }

            // convert phase in [0,1) into radians offset
            double phase = phase01 * 2.0 * System.Math.PI;

            if (hard)
            {
                // hard strobe: on/off with duty cycle
                double cycle = (timeSeconds * hz + phase01) % 1.0;
                return (cycle < duty01) ? 1.0f : 0.0f;
            }
            else
            {
                // soft shimmer: sinusoidal brightness (0..1)
                double x = 2.0 * System.Math.PI * hz * timeSeconds + phase;
                double s = 0.5 + 0.5 * System.Math.Sin(x);
                return (float)s;
            }
        }
    }
}