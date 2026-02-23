using _08_Fireworks.Config;
using _08_Fireworks.Core;
using _08_Fireworks.Math;
using _08_Fireworks.Particles;
using _08_Fireworks.Spawning;

namespace _08_Fireworks.Effects
{
    public sealed class CrackleSystem
    {
        private readonly FireworksConfig _cfg;

        public int CrackleEventsThisFrame { get; private set; }
        public int MicroSparksSpawnedThisFrame { get; private set; }

        public CrackleSystem (FireworksConfig cfg)
        {
            _cfg = cfg;
        }

        public void Step (double dt, ParticlePool pool, BurstSpawner spawner, IRandSource rng, bool crackleEnabled)
        {
            CrackleEventsThisFrame = 0;
            MicroSparksSpawnedThisFrame = 0;

            if (!crackleEnabled || dt <= 0.0)
            {
                return;
            }

            var cSpark = _cfg.Spark;
            var cMicro = _cfg.MicroSpark;

            var alive = pool.AliveIds;

            for (int i = 0; i < alive.Length; i++)
            {
                int id = alive[i];
                ref var p = ref pool.Get(id);

                if (p.Kind != ParticleKind.Spark)
                {
                    continue;
                }

                if (!p.IsAlive)
                {
                    continue;
                }

                var rate = (double)p.CrackleRate;
                if (rate <= 0.0)
                {
                    continue;
                }

                bool fireEvent = Distributions.EventFromRate(rng, rate, dt);
                if (!fireEvent)
                {
                    continue;
                }

                CrackleEventsThisFrame++;

                int burstCount = rng.NextInt(cSpark.CrackleBurstCount.Min, cSpark.CrackleBurstCount.Max);

                // Micro spark speed range from MicroSpark config
                var spawned = spawner.SpawnMicroSparks(rng, pool, p,burstCount, cMicro.Speed.Min, cMicro.Speed.Max);

                MicroSparksSpawnedThisFrame += spawned;
            }
        }
    }
}