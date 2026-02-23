namespace _08_Fireworks.Config
{
    public sealed class RuntimeControls
    {
        public bool StrobeEnabled = true;
        public bool CrackleEnabled = true;

        // rates / intensities
        public double RocketRatePerSecond = 2.0;
        public double GlobalTimeScale = 1.0;

        // global debug toggles
        public bool FreezeSimulation = false;
        public bool PauseSpawning = false;
    }
}