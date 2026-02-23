namespace _08_Fireworks.Core
{
    public sealed class SimClock
    {
        public double Time { get; private set; }
        public double LastDt { get; private set; }

        private bool _initialized;

        public void Reset ()
        {
            Time = 0.0;
            LastDt = 0.0;
            _initialized = false;
        }

        public double StepTo (double newTime, double maxDt)
        {
            if (!_initialized)
            {
                _initialized = true;
                Time = newTime;
                LastDt = 0.0;
                return 0.0;
            }

            var dt = newTime - Time;

            if (dt < 0.0)
            {
                dt = 0.0;
            }

            if (dt > maxDt)
            {
                dt = maxDt;
            }

            Time = newTime;
            LastDt = dt;

            return dt;
        }
    }
}