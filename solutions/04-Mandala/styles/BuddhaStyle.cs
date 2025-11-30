using System;
using _04Mandala.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _04Mandala.Styles
{
    public class BuddhaStyle : IMandalaStyle
    {
        public MandalaStyleKind Kind => MandalaStyleKind.Buddha;

        public void Render (MandalaConfig config, Image<Rgba32> image)
        {
            var ctx = new BuddhaContext(config, image);
            ctx.Render();
        }

        private sealed class BuddhaContext
        {
            private readonly Image<Rgba32> _image;

            private readonly int _width;
            private readonly int _height;
            private readonly int _symmetry;
            private readonly int _seed;

            private readonly float _cx;
            private readonly float _cy;
            private readonly float _radiusMax;

            private readonly float _detail;
            private readonly float _wedgeSize;

            private readonly int _ringCount;
            private readonly float[] _ringRadius;
            private readonly float[] _ringWidth;
            private readonly bool[] _ringSegmented;
            private readonly int[] _ringSegmentsPerWedge;

            private readonly int _squareCount;
            private readonly float[] _squareRadius;
            private readonly float[] _squareWidth;

            private readonly bool _hasPlusCross;
            private readonly bool _hasDiagonalCross;
            private readonly float _plusWidth;
            private readonly float _plusMaxR;
            private readonly float _diagWidth;
            private readonly float _diagMaxR;

            private readonly float _rayWidth;
            private readonly float _rayInnerSkip;

            public BuddhaContext (MandalaConfig config, Image<Rgba32> image)
            {
                _image = image;

                _width = config.Width;
                _height = config.Height;
                _symmetry = Math.Max(4, config.Symmetry);
                _seed = config.Seed ?? 0;

                _cx = _width / 2f;
                _cy = _height / 2f;
                _radiusMax = MathF.Min(_width, _height) / 2f;

                _detail = (float)config.Detail;
                _wedgeSize = 2f * MathF.PI / _symmetry;

                var rng = new Random(_seed);

                _ringCount = 5 + (int)(_detail * 4f);
                if (_ringCount < 5)
                    _ringCount = 5;

                _ringRadius = new float[_ringCount];
                _ringWidth = new float[_ringCount];
                _ringSegmented = new bool[_ringCount];
                _ringSegmentsPerWedge = new int[_ringCount];

                InitialiseRings(rng);

                _squareCount = 2 + rng.Next(3);
                _squareRadius = new float[_squareCount];
                _squareWidth = new float[_squareCount];

                InitialiseSquares(rng);

                _hasPlusCross = rng.NextDouble() < 0.85;
                _hasDiagonalCross = rng.NextDouble() < 0.55;

                _plusWidth = 0.035f + 0.03f * (float)rng.NextDouble();
                _plusMaxR = 0.32f + 0.26f * (float)rng.NextDouble();

                _diagWidth = 0.03f + 0.03f * (float)rng.NextDouble();
                _diagMaxR = 0.26f + 0.32f * (float)rng.NextDouble();

                _rayWidth = 0.03f + 0.03f * (float)rng.NextDouble();
                _rayInnerSkip = 0.10f + 0.08f * (float)rng.NextDouble();
            }

            private void InitialiseRings (Random rng)
            {
                float inner = 0.20f;
                float outer = 0.95f;
                float span = outer - inner;

                for (int i = 0; i < _ringCount; i++)
                {
                    float t = (_ringCount == 1) ? 0f : i / (float) (_ringCount - 1);

                    float baseR = inner + span * t;
                    float jitter = ((float) rng.NextDouble() - 0.5f) * 0.02f;
                    _ringRadius[i] = baseR + jitter;

                    float baseW = 0.006f + 0.004f * t;
                    float wJitter = ((float) rng.NextDouble() - 0.5f) * 0.004f;
                    float width = baseW + wJitter;
                    if (width < 0.003f)
                        width = 0.003f;
                    _ringWidth[i] = width;

                    double segProb = 0.25 + 0.5 * t;
                    bool segmented = rng.NextDouble() < segProb;
                    _ringSegmented[i] = segmented;

                    if (segmented)
                    {
                        _ringSegmentsPerWedge[i] = 1 + rng.Next(3);
                    }
                    else
                    {
                        _ringSegmentsPerWedge[i] = 0;
                    }
                }
            }

            private void InitialiseSquares (Random rng)
            {
                float minR = 0.14f;
                float maxR = 0.55f;
                float span = maxR - minR;

                float step = span / (_squareCount + 1);

                for (int i = 0; i < _squareCount; i++)
                {
                    float basis = minR + step * (i + 1);
                    float jitter = ((float) rng.NextDouble() - 0.5f) * step * 0.35f;
                    _squareRadius[i] = basis + jitter;

                    float t = (_squareCount == 1) ? 0f : i / (float) (_squareCount - 1);
                    float baseW = 0.013f - 0.004f * t;
                    float wJitter = ((float) rng.NextDouble() - 0.5f) * 0.004f;
                    float width = baseW + wJitter;
                    if (width < 0.006f)
                        width = 0.006f;
                    _squareWidth[i] = width;
                }

                Array.Sort(_squareRadius, _squareWidth);
            }

            public void Render ()
            {
                _image.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < _height; y++)
                    {
                        var row = accessor.GetRowSpan(y);
                        for (int x = 0; x < _width; x++)
                        {
                            row[x] = RenderPixel(x, y);
                        }
                    }
                });
            }

            private Rgba32 RenderPixel (int x, int y)
            {
                float dx = x - _cx;
                float dy = y - _cy;
                float r = MathF.Sqrt(dx * dx + dy * dy);
                float rNorm = r / _radiusMax;

                if (rNorm > 1f)
                {
                    return new Rgba32(2, 4, 8);
                }

                float angle = MathF.Atan2(dy, dx);
                if (angle < 0f)
                {
                    angle += 2f * MathF.PI;
                }

                float foldedAngle = angle % _wedgeSize;
                float wedgeNorm = foldedAngle / _wedgeSize;
                Rgba32 baseColor = BackgroundColor(rNorm);

                float mask = 0f;
                mask = MathF.Max(mask, RingsMask(rNorm, wedgeNorm));
                mask = MathF.Max(mask, SquaresMask(dx, dy));
                mask = MathF.Max(mask, CrossMask(dx, dy, rNorm));
                mask = MathF.Max(mask, RaysMask(rNorm, wedgeNorm));

                if (rNorm < 0.12f)
                {
                    float centreBoost = 0.85f + 0.15f * GildedNoise(rNorm, wedgeNorm);
                    mask = MathF.Max(mask, centreBoost);
                }

                float noise = GildedNoise(rNorm, wedgeNorm);
                float goldMask = MathExtensions.Clamp(
                    mask * (0.85f + 0.35f * (noise - 0.5f)),
                    0f, 1f
                );

                Rgba32 goldColor = GoldColor(rNorm);

                float a = goldMask;
                float outR = baseColor.R * (1f - a) + goldColor.R * a;
                float outG = baseColor.G * (1f - a) + goldColor.G * a;
                float outB = baseColor.B * (1f - a) + goldColor.B * a;

                byte rr = (byte) MathExtensions.Clamp(outR, 0f, 255f);
                byte gg = (byte) MathExtensions.Clamp(outG, 0f, 255f);
                byte bb = (byte) MathExtensions.Clamp(outB, 0f, 255f);

                return new Rgba32(rr, gg, bb);
            }

            private float RingsMask (float rNorm, float wedgeNorm)
            {
                float best = 0f;

                for (int i = 0; i < _ringCount; i++)
                {
                    float radius = _ringRadius[i];
                    float width = _ringWidth[i];

                    float d = MathF.Abs(rNorm - radius);
                    if (d > width)
                        continue;

                    float local = 1f - d / width;

                    if (_ringSegmented[i] && _ringSegmentsPerWedge[i] > 0)
                    {
                        float segPos = wedgeNorm * _ringSegmentsPerWedge[i];
                        int segIndex = (int) MathF.Floor(segPos);

                        if ((segIndex & 1) == 0)
                        {
                            local *= 0.3f;
                        }
                    }

                    if (local > best)
                        best = local;
                }

                return best;
            }

            private float SquaresMask (float dx, float dy)
            {
                float sNorm = MathF.Max(MathF.Abs(dx), MathF.Abs(dy)) / _radiusMax;
                float best = 0f;

                for (int i = 0; i < _squareCount; i++)
                {
                    float radius = _squareRadius[i];
                    float width = _squareWidth[i];

                    float d = MathF.Abs(sNorm - radius);
                    if (d > width)
                        continue;

                    float local = 1f - d / width;
                    if (local > best)
                        best = local;
                }

                return best;
            }

            private float CrossMask (float dx, float dy, float rNorm)
            {
                float mask = 0f;

                float nx = dx / _radiusMax;
                float ny = dy / _radiusMax;

                if (_hasPlusCross && rNorm < _plusMaxR)
                {
                    float ax = MathF.Abs(nx);
                    float ay = MathF.Abs(ny);

                    if (ax < _plusWidth)
                    {
                        float local = 1f - ax / _plusWidth;
                        if (local > mask)
                            mask = local;
                    }

                    if (ay < _plusWidth)
                    {
                        float local = 1f - ay / _plusWidth;
                        if (local > mask)
                            mask = local;
                    }
                }

                if (_hasDiagonalCross && rNorm < _diagMaxR)
                {
                    const float invSqrt2 = 0.70710678f;

                    float along1 = (nx + ny) * invSqrt2;
                    float along2 = (nx - ny) * invSqrt2;

                    float d1 = MathF.Abs(along1);
                    float d2 = MathF.Abs(along2);

                    if (d1 < _diagWidth)
                    {
                        float local = 1f - d1 / _diagWidth;
                        if (local > mask)
                            mask = local;
                    }

                    if (d2 < _diagWidth)
                    {
                        float local = 1f - d2 / _diagWidth;
                        if (local > mask)
                            mask = local;
                    }
                }

                return mask;
            }

            private float RaysMask (float rNorm, float wedgeNorm)
            {
                if (rNorm < _rayInnerSkip)
                    return 0f;

                float distToBoundary = MathF.Min(wedgeNorm, 1f - wedgeNorm);
                if (distToBoundary > _rayWidth)
                    return 0f;

                float angular = 1f - distToBoundary / _rayWidth;

                float t = (rNorm - _rayInnerSkip) / (1f - _rayInnerSkip);
                if (t < 0f)
                    t = 0f;
                if (t > 1f)
                    t = 1f;
                float radialFade = 0.3f + 0.7f * (1f - t);

                return angular * radialFade;
            }

            private Rgba32 BackgroundColor (float rNorm)
            {
                float halo = MathF.Pow(MathF.Max(1f - rNorm * 1.05f, 0f), 1.6f);

                float r = 6f + 22f * halo;
                float g = 8f + 30f * halo;
                float b = 12f + 52f * halo;

                byte rr = (byte) MathExtensions.Clamp(r, 0f, 255f);
                byte gg = (byte) MathExtensions.Clamp(g, 0f, 255f);
                byte bb = (byte) MathExtensions.Clamp(b, 0f, 255f);

                return new Rgba32(rr, gg, bb);
            }

            private float GildedNoise (float rNorm, float wedgeNorm)
            {
                float v = rNorm * 40f + wedgeNorm * 11f + _seed * 0.017f;
                float s = MathF.Sin(v) + 0.5f * MathF.Sin(2f * v + 1.37f);
                float n = s * 0.5f + 0.5f;
                return n;
            }

            private Rgba32 GoldColor (float rNorm)
            {
                float centreBoost = MathF.Pow(MathF.Max(1f - rNorm * 0.9f, 0f), 1.1f);

                float intensity = 0.6f + 0.4f * centreBoost;
                float r = 205f * intensity;
                float g = 178f * intensity;
                float b = 90f * intensity + 35f * (1f - rNorm);

                byte rr = (byte) MathExtensions.Clamp(r, 0f, 255f);
                byte gg = (byte) MathExtensions.Clamp(g, 0f, 255f);
                byte bb = (byte) MathExtensions.Clamp(b, 0f, 255f);

                return new Rgba32(rr, gg, bb);
            }
        }
    }
}