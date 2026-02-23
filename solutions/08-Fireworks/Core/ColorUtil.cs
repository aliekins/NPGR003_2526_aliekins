using Silk.NET.Maths;

namespace _08_Fireworks.Core
{
    public static class ColorUtil
    {
        public static Vector3D<float> Lerp (Vector3D<float> first, Vector3D<float> second, float t)
        {
            if (t < 0f)
            {
                t = 0f;
            }

            if (t > 1f)
            {
                t = 1f;
            }

            return first + (second - first) * t;
        }

        public static Vector3D<float> Scale (Vector3D<float> color, float s)
        {
            return new Vector3D<float>(color.X * s, color.Y * s, color.Z * s);
        }

        public static float Clamp01 (float x)
        {
            if (x < 0f)
            {
                return 0f;
            }
            if (x > 1f)
            {
                return 1f;
            }
            return x;
        }

        public static Vector3D<float> Clamp01 (Vector3D<float> c)
        {
            return new Vector3D<float>(Clamp01(c.X), Clamp01(c.Y), Clamp01(c.Z));
        }

        public static Vector3D<float> ColorOverLife (Vector3D<float> baseColor, Vector3D<float> drift, float normalizedAge, float seed01)
        {
            float t = normalizedAge;
            var target = Clamp01(baseColor + drift);
            float u = t * (2.0f - t);
            float wobble = 0.9f + 0.1f * (float)System.Math.Sin(t * 10.0f + seed01 * 6.283185f);

            var c = Lerp(baseColor, target, u * wobble);

            float fade = 1.0f - t;
            c = Scale(c, (float)System.Math.Pow(fade, 0.02f));

            return Clamp01(c);
        }
    }
}