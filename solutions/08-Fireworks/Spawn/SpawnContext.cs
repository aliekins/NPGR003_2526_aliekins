using _08_Fireworks.Config;
using _08_Fireworks.Math;
using _08_Fireworks.Particles;

namespace _08_Fireworks.Spawn
{
    public readonly struct SpawnContext
    {
        public readonly FireworksConfig Config;
        public readonly RuntimeControls Runtime;
        public readonly IRandSource Rng;
        public readonly ParticlePool Pool;

        public SpawnContext (FireworksConfig config, RuntimeControls runtime, IRandSource rng, ParticlePool pool)
        {
            Config = config;
            Runtime = runtime;
            Rng = rng;
            Pool = pool;
        }
    }
}