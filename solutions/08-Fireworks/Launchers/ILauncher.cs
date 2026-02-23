using _08_Fireworks.Config;
using _08_Fireworks.Math;
using _08_Fireworks.Particles;
using _08_Fireworks.Spawning;

namespace _08_Fireworks.Launchers
{
    public interface ILauncher
    {
        bool Enabled { get; set; }

        // Called once per frame
        void Step (
            double dt,
            in LauncherInput input,
            ParticlePool pool,
            ParticleFactory factory,
            FireworksConfig cfg,
            RuntimeControls runtime,
            IRandSource rng);
    }
}
