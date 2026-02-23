using System.Collections.Generic;
using _08_Fireworks.Config;
using _08_Fireworks.Math;
using _08_Fireworks.Particles;
using _08_Fireworks.Spawning;

namespace _08_Fireworks.Launchers
{
    public sealed class LauncherController
    {
        private readonly List<ILauncher> _launchers = new List<ILauncher>();

        public void Add (ILauncher launcher)
        {
            _launchers.Add(launcher);
        }

        public void Step (double dt, in LauncherInput input, ParticlePool pool, ParticleFactory factory, FireworksConfig cfg, RuntimeControls runtime, IRandSource rng)
        {
            for (int i = 0; i < _launchers.Count; i++)
            {
                _launchers[i].Step(dt, input, pool, factory, cfg, runtime, rng);
            }
        }
    }
}