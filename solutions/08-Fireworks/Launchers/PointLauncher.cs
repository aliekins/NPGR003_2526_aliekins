using _08_Fireworks.Config;
using _08_Fireworks.Core;
using _08_Fireworks.Math;
using _08_Fireworks.Particles;
using _08_Fireworks.Spawning;
using Silk.NET.Maths;

namespace _08_Fireworks.Launchers
{
    public sealed class PointLauncher : ILauncher
    {
        public bool Enabled { get; set; } = true;

        public Vector3D<double> Position;

        // Launch direction defaults
        public Vector3D<double> BaseDirection = new Vector3D<double>(0.0, 1.0, 0.0);
        public double ConeAngleRad = 0.15; // ~8.6 degrees

        private double _fireAccumulator;

        public PointLauncher (Vector3D<double> position)
        {
            Position = position;
        }

        public void Step (double dt, in LauncherInput input, ParticlePool pool, ParticleFactory factory, FireworksConfig cfg, RuntimeControls runtime, IRandSource rng)
        {
            if (!Enabled)
            {
                return;
            }
            if (dt <= 0.0)
            {
                return;
            }

            bool fire = input.FireHeld || input.FirePressed;
            if (!fire)
            {
                _fireAccumulator = 0.0;
                return;
            }

            // Spawn throttling near capacity
            double fill = cfg.MaxParticles == 0 ? 1.0 : (double)pool.AliveCount / cfg.MaxParticles;
            double throttle = SpawnThrottle.Compute(fill, cfg.SpawnThrottleStartRatio, cfg.SpawnThrottleHardRatio);

            if (throttle <= 0.0)
            {
                return;
            }

            // Effective rate 
            double rate = runtime.RocketRatePerSecond * throttle;
            if (rate <= 0.0)
            {
                return;
            }

            _fireAccumulator += dt * rate;

            // Spawn floor(accumulator) rockets this frame
            int toSpawn = (int)_fireAccumulator;
            if (toSpawn <= 0)
            {
                return;
            }

            _fireAccumulator -= toSpawn;

            // Fire rockets
            for (int i = 0; i < toSpawn; i++)
            {
                if (!pool.TryAllocate(out int id))
                {
                    break;
                }

                var axis = input.AimDirection.Length > 1e-9 ? input.AimDirection : BaseDirection;
                axis = axis / axis.Length;

                var dir = Sample3D.InCone(rng, axis, ConeAngleRad);
                double dlen = dir.Length;
                if (dlen > 1e-12)
                {
                    dir /= dlen;
                }

                var speed = rng.Next(cfg.Rocket.Speed.Min, cfg.Rocket.Speed.Max);
                var vel = dir * speed;

                var color = ColorSampling.PickPalette(rng);

                var rocket = factory.CreateRocket(rng, Position, vel, color);
                pool.Get(id) = rocket;
            }
        }
    }
}