using System;

namespace _04Mandala.Drawing
{
    public readonly struct PolarCoordinate
    {
        public float Radius { get; }
        public float Angle { get; }

        public PolarCoordinate (float radius, float angle)
        {
            Radius = radius;
            Angle = angle;
        }
    }

    public static class CoordinateMapper
    {
        public static PolarCoordinate ToPolar (int x, int y, int width, int height)
        {
            float centerX = width / 2f;
            float centerY = height / 2f;
            float dx = x - centerX;
            float dy = y - centerY;

            float radius = MathF.Sqrt(dx * dx + dy * dy);
            float angle = MathF.Atan2(dy, dx);
            if (angle < 0)
            {
                angle += 2f * MathF.PI;
            }

            return new PolarCoordinate(radius, angle);
        }

        public static float GetNormalizedRadius (PolarCoordinate polar, int width, int height)
        {
            float maxRadius = MathF.Min(width, height) / 2f;
            return polar.Radius / maxRadius;
        }

        public static float FoldAngleToWedge (float angle, int symmetry)
        {
            float wedgeSize = 2f * MathF.PI / symmetry;
            float foldedAngle = angle % wedgeSize;

            if (foldedAngle < 0)
            {
                foldedAngle += wedgeSize;
            }

            return foldedAngle;
        }
    }
}