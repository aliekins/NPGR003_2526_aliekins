using Silk.NET.Maths;

namespace _08_Fireworks.Math
{
    public static class ColorSampling
    {
        private static readonly Vector3D<float>[] Palette =
        {
            new Vector3D<float>(1f, 0.2f, 0.2f),  // red
            new Vector3D<float>(0.2f, 1f, 0.2f),  // green
            new Vector3D<float>(0.2f, 0.4f, 1f),  // blue
            new Vector3D<float>(1f, 1f, 0.2f),    // yellow
            new Vector3D<float>(1f, 0.4f, 1f),    // magenta
            new Vector3D<float>(0.2f, 1f, 1f),    // cyan
            new Vector3D<float>(1f, 0.6f, 0.2f),  // orange
        };

        public static Vector3D<float> PickPalette (IRandSource rng)
        {
            var i = rng.NextInt(0, Palette.Length - 1);
            return Palette[i];
        }
    }
}