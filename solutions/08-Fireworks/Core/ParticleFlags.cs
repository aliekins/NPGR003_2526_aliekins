using System;

namespace _08_Fireworks.Core
{
    [Flags]
    public enum ParticleFlags : ushort
    {
        None = 0,
        Exploded = 1 << 0
    }
}