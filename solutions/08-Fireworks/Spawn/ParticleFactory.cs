using _08_Fireworks.Config;
using _08_Fireworks.Core;
using _08_Fireworks.Math;
using Silk.NET.Maths;

namespace _08_Fireworks.Spawning
{
    public sealed class ParticleFactory
    {
        private readonly FireworksConfig _cfg;

        public ParticleFactory (FireworksConfig cfg)
        {
            _cfg = cfg;
        }

        public Particle CreateRocket (IRandSource rng, Vector3D<double> position, Vector3D<double> initialVelocity, Vector3D<float> color)
        {
            var c = _cfg.Rocket;

            return new Particle
            {
                Kind = ParticleKind.Rocket,
                Flags = ParticleFlags.None,

                Position = position,
                Velocity = initialVelocity,

                BaseColor = color,
                ColorDrift = RandomDrift(rng, 0.30f),
                Color = color,
                Size = 12.0f,

                Age = 0.0,
                Life = rng.Next(c.Life.Min, c.Life.Max),

                Mass = c.Mass,
                DragLinear = c.DragLinear,
                DragQuadratic = c.DragQuadratic,

                Fuse = rng.Next(c.Fuse.Min, c.Fuse.Max),
                Stage = 0,

                StrobeHz = 0.0f,
                StrobePhase = (float)rng.Next01(),
                CrackleRate = 0.0f
            };
        }

        public Particle CreateBomblet (IRandSource rng, Vector3D<double> position, Vector3D<double> initialVelocity, Vector3D<float> color)
        {
            var c = _cfg.Bomblet;

            return new Particle
            {
                Kind = ParticleKind.Bomblet,
                Flags = ParticleFlags.None,

                Position = position,
                Velocity = initialVelocity,

                BaseColor = color,
                ColorDrift = RandomDrift(rng, 0.45f),
                Color = color,
                Size = 8.0f,

                Age = 0.0,
                Life = rng.Next(c.Life.Min, c.Life.Max),

                Mass = c.Mass,
                DragLinear = c.DragLinear,
                DragQuadratic = c.DragQuadratic,

                Fuse = rng.Next(c.Fuse.Min, c.Fuse.Max),
                Stage = 1,

                StrobeHz = 0.0f,
                StrobePhase = (float)rng.Next01(),
                CrackleRate = 0.0f
            };
        }

        public Particle CreateSpark (IRandSource rng, Vector3D<double> position, Vector3D<double> initialVelocity, Vector3D<float> color)
        {
            var c = _cfg.Spark;

            var strobeHz = rng.Next(c.StrobeHz.Min, c.StrobeHz.Max);
            var crackleRate = rng.Next(c.CrackleRate.Min, c.CrackleRate.Max);

            return new Particle
            {
                Kind = ParticleKind.Spark,
                Flags = ParticleFlags.None,

                Position = position,
                Velocity = initialVelocity,

                BaseColor = color,
                ColorDrift = RandomDrift(rng, 0.65f),
                Color = color,
                Size = (float)rng.Next(c.Size.Min, c.Size.Max),

                Age = 0.0,
                Life = rng.Next(c.Life.Min, c.Life.Max),

                Mass = c.Mass,
                DragLinear = c.DragLinear,
                DragQuadratic = c.DragQuadratic,

                Fuse = double.PositiveInfinity, // sparks do not explode
                Stage = 2,

                StrobeHz = (float)strobeHz,
                StrobePhase = (float)rng.Next01(),
                CrackleRate = (float)crackleRate
            };
        }

        public Particle CreateMicroSpark (IRandSource rng, Vector3D<double> position, Vector3D<double> initialVelocity, Vector3D<float> color)
        {
            var c = _cfg.MicroSpark;

            return new Particle
            {
                Kind = ParticleKind.MicroSpark,
                Flags = ParticleFlags.None,

                Position = position,
                Velocity = initialVelocity,

                BaseColor = color,
                ColorDrift = RandomDrift(rng, 0.85f),
                Color = color,
                Size = (float)rng.Next(c.Size.Min, c.Size.Max),

                Age = 0.0,
                Life = rng.Next(c.Life.Min, c.Life.Max),

                Mass = c.Mass,
                DragLinear = c.DragLinear,
                DragQuadratic = c.DragQuadratic,

                Fuse = double.PositiveInfinity,
                Stage = 3,

                StrobeHz = 0.0f,
                StrobePhase = (float)rng.Next01(),
                CrackleRate = 0.0f
            };
        }

        private static Vector3D<float> RandomDrift (IRandSource rng, float magnitude)
        {
            float x = (float)(rng.Next01() * 2.0 - 1.0);
            float y = (float)(rng.Next01() * 2.0 - 1.0);
            float z = (float)(rng.Next01() * 2.0 - 1.0);
            return new Vector3D<float>(x * magnitude, y * magnitude, z * magnitude);
        }
    }
}