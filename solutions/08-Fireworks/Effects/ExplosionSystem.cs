using _08_Fireworks.Core;
using _08_Fireworks.Math;
using _08_Fireworks.Particles;
using _08_Fireworks.Spawning;

namespace _08_Fireworks.Effects
{
    public sealed class ExplosionSystem
    {
        // debugging
        public int ExplosionsThisFrame { get; private set; }

        public void Step (double dt, ParticlePool pool, BurstSpawner spawner, IRandSource rng)
        {
            ExplosionsThisFrame = 0;

            // Iterate backwards - might Free() the current id
            var alive = pool.AliveIds;

            for (int i = alive.Length - 1; i >= 0; i--)
            {
                int id = alive[i];
                ref var p = ref pool.Get(id);

                if (!p.IsAlive)
                {
                    continue;
                }

                // only Rocket and Bomblet explode
                if (p.Kind != ParticleKind.Rocket && p.Kind != ParticleKind.Bomblet)
                {
                    continue;
                }

                // explode exactly once
                if ((p.Flags & ParticleFlags.Exploded) != 0)
                {
                    continue;
                }

                // fuse trigger
                if (p.Age < p.Fuse)
                {
                    continue;
                }
                p.Flags |= ParticleFlags.Exploded;

                // Spawn children
                if (p.Kind == ParticleKind.Rocket)
                {
                    spawner.SpawnBomblets(rng, pool, p);
                    ExplosionsThisFrame++;

                    // retire after explosion
                    pool.Free(id);
                }
                else // Bomblet
                {
                    spawner.SpawnSparks(rng, pool, p);
                    ExplosionsThisFrame++;
                    pool.Free(id);
                }
            }
        }
    }
}