using Silk.NET.Maths;

namespace _08_Fireworks.Core
{
    public struct Particle
    {
        public ParticleKind Kind;
        public ParticleFlags Flags;

        // physics state
        public Vector3D<double> Position;
        public Vector3D<double> Velocity;

        // Appearance
        public Vector3D<float> BaseColor;
        public Vector3D<float> ColorDrift;
        public Vector3D<float> Color;
        public float Size;

        // Lifetime
        public double Age; // s since spawn
        public double Life; // s till death

        // phy params
        public double Mass;
        public double DragLinear;
        public double DragQuadratic;

        // Explosion trigger
        public double Fuse;  // explode when Age >= Fuse
        public byte Stage;

        public float StrobeHz;
        public float StrobePhase;

        public float CrackleRate;

        // helpers
        public readonly bool IsAlive => Age < Life;
        public readonly float NormalizedAge
        {
            get
            {
                if (Life <= 1e-9)
                {
                    return 1.0f;
                }

                var t = (float)(Age / Life);

                if (t < 0f)
                {
                    return 0f;
                }

                if (t > 1f)
                {
                    return 1f;
                }
                return t;
            }
        }

    }
}