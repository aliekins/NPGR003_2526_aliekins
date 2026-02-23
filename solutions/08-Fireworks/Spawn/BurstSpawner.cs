using _08_Fireworks.Config;
using _08_Fireworks.Core;
using _08_Fireworks.Math;
using _08_Fireworks.Particles;
using Silk.NET.Maths;

namespace _08_Fireworks.Spawning
{
    public sealed class BurstSpawner
    {
        private readonly FireworksConfig _cfg;
        private readonly ParticleFactory _factory;

        public BurstSpawner (FireworksConfig cfg, ParticleFactory factory)
        {
            _cfg = cfg;
            _factory = factory;
        }

        // Rocket explosion - bomblets
        public int SpawnBomblets (IRandSource rng, ParticlePool pool, in Particle rocket)
        {
            var cR = _cfg.Rocket;

            int count = rng.NextInt(cR.BombletCount.Min, cR.BombletCount.Max);
            int spawned = 0;

            for (int i = 0; i < count; i++)
            {
                if (!pool.TryAllocate(out int id))
                {
                    break;
                }

                var dir = Sample3D.UnitVector(rng);
                var speed = rng.Next(cR.BombletBurstSpeed.Min, cR.BombletBurstSpeed.Max);

                var vel = dir * speed + rocket.Velocity * cR.InheritVelocity;
                var p = _factory.CreateBomblet(rng, rocket.Position, vel, rocket.BaseColor);

                pool.Get(id) = p;
                spawned++;
            }

            return spawned;
        }

        // Bomblet explosion - sparks
        public int SpawnSparks (IRandSource rng, ParticlePool pool, in Particle bomblet)
        {
            var cB = _cfg.Bomblet;

            int count = rng.NextInt(cB.SparkCount.Min, cB.SparkCount.Max);
            int spawned = 0;

            for (int i = 0; i < count; i++)
            {
                if (!pool.TryAllocate(out int id))
                {
                    break;
                }

                var dir = Sample3D.UnitVector(rng);
                var speed = rng.Next(cB.SparkBurstSpeed.Min, cB.SparkBurstSpeed.Max);

                var vel = dir * speed + bomblet.Velocity * cB.InheritVelocity;
                var p = _factory.CreateSpark(rng, bomblet.Position, vel, bomblet.BaseColor);

                pool.Get(id) = p;
                spawned++;
            }

            return spawned;
        }

        // Spark crackle event - micro sparks
        public int SpawnMicroSparks (IRandSource rng, ParticlePool pool, in Particle spark, int count, double speedMin, double speedMax)
        {
            int spawned = 0;

            for (int i = 0; i < count; i++)
            {
                if (!pool.TryAllocate(out int id))
                {
                    break;
                }

                var dir = Sample3D.UnitVector(rng);
                var speed = rng.Next(speedMin, speedMax);

                var vel = spark.Velocity * 0.6 + dir * speed;

                var microColor = spark.BaseColor;
                var p = _factory.CreateMicroSpark(rng, spark.Position, vel, microColor);
                pool.Get(id) = p;
                spawned++;
            }

            return spawned;
        }
    }
}