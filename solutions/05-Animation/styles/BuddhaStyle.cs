using System;
using _05Animation.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _05Animation.Styles
{
    public class BuddhaStyle : IMandalaStyle
    {
        public MandalaStyleKind Kind => MandalaStyleKind.Buddha;

        public void Render (MandalaConfig config, Image<Rgba32> image, float time)
        {
            var ctx = new BuddhaContext(config, image, time);
            ctx.Render();
        }

        private sealed class BuddhaContext
        {
            private readonly Image<Rgba32> _image;

            private readonly int _width;
            private readonly int _height;
            private readonly int _symmetry;
            private readonly int _symmetry2;
            private readonly int _seed;

            private readonly float _cx;
            private readonly float _cy;
            private readonly float _radiusMax;

            private readonly float _detail;
            private readonly float _wedgeSize;
            private readonly float _wedgeSize2;

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

            private readonly float _phase;
            private readonly float _loop;
            private readonly float _loop2;
            private readonly float _signed;

            public BuddhaContext (MandalaConfig config, Image<Rgba32> image, float time)
            {
                _image = image;

                _width = config.Width;
                _height = config.Height;

                _symmetry = Math.Max(4, config.Symmetry);
                _symmetry2 = _symmetry + 2;

                _seed = config.Seed ?? 0;

                _cx = _width / 2f;
                _cy = _height / 2f;
                _radiusMax = MathF.Min(_width, _height) / 2f;

                _detail = (float)config.Detail;

                _wedgeSize = 2f * MathF.PI / _symmetry;
                _wedgeSize2 = 2f * MathF.PI / _symmetry2;

                float t = MathExtensions.Clamp01(time);
                _phase = 2f * MathF.PI * t;
                _loop = 0.5f - 0.5f * MathF.Cos(_phase);
                _loop2 = 0.5f - 0.5f * MathF.Cos(2f * _phase);
                _signed = 2f * _loop - 1f;

                var rng = new Random(_seed);

                _ringCount = 5 + (int)(_detail * 4f);
                if (_ringCount < 5)
                {
                    _ringCount = 5;
                }

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
                    float t = (_ringCount == 1) ? 0f : i / (float)(_ringCount - 1);

                    float baseR = inner + span * t;
                    float jitter = ((float)rng.NextDouble() - 0.5f) * 0.02f;
                    _ringRadius[i] = baseR + jitter;

                    float baseW = 0.006f + 0.004f * t;
                    float wJitter = ((float)rng.NextDouble() - 0.5f) * 0.004f;
                    float width = baseW + wJitter;
                    if (width < 0.003f)
                    {
                        width = 0.003f;
                    }
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
                    float jitter = ((float)rng.NextDouble() - 0.5f) * step * 0.35f;
                    _squareRadius[i] = basis + jitter;

                    float t = (_squareCount == 1) ? 0f : i / (float)(_squareCount - 1);
                    float baseW = 0.013f - 0.004f * t;
                    float wJitter = ((float)rng.NextDouble() - 0.5f) * 0.004f;
                    float width = baseW + wJitter;
                    if (width < 0.006f)
                    {
                        width = 0.006f;
                    }
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

                float foldedA = angle % _wedgeSize;
                float foldedB = angle % _wedgeSize2;

                float wedgeNormA = foldedA / _wedgeSize;
                float wedgeNormB = foldedB / _wedgeSize2;

                float wedgeNorm = wedgeNormA * (1f - _loop2) + wedgeNormB * _loop2;

                Rgba32 baseColor = BackgroundColor(rNorm);

                float ringWidthScale = 0.85f + 0.35f * _loop;
                float squareWidthScale = 0.88f + 0.30f * _loop2;
                float rayWidthScale = 0.85f + 0.35f * _loop;

                float mask = 0f;
                mask = MathF.Max(mask, RingsMask(rNorm, wedgeNorm, ringWidthScale));
                mask = MathF.Max(mask, SquaresMask(dx, dy, squareWidthScale));
                mask = MathF.Max(mask, CrossMask(dx, dy, rNorm, squareWidthScale));
                mask = MathF.Max(mask, RaysMask(rNorm, wedgeNorm, rayWidthScale));

                if (rNorm < 0.12f)
                {
                    float centreBoost = 0.82f + 0.18f * (0.5f + 0.5f * MathF.Sin(_phase + wedgeNorm * 6f));
                    mask = MathF.Max(mask, centreBoost);
                }

                float noise = GildedNoise(rNorm, wedgeNorm);
                float goldMask = MathExtensions.Clamp(  mask * (0.80f + 0.30f * _loop + 0.25f * (noise - 0.5f)), 0f, 1f );

                Rgba32 goldColor = GoldColor(rNorm);

                float a = goldMask;
                float outR = baseColor.R * (1f - a) + goldColor.R * a;
                float outG = baseColor.G * (1f - a) + goldColor.G * a;
                float outB = baseColor.B * (1f - a) + goldColor.B * a;

                byte rr = (byte)MathExtensions.Clamp(outR, 0f, 255f);
                byte gg = (byte)MathExtensions.Clamp(outG, 0f, 255f);
                byte bb = (byte)MathExtensions.Clamp(outB, 0f, 255f);

                return new Rgba32(rr, gg, bb);
            }

            private float RingsMask (float rNorm, float wedgeNorm, float widthScale)
            {
                float best = 0f;

                for (int i = 0; i < _ringCount; i++)
                {
                    float radius = _ringRadius[i];

                    float t = (_ringCount == 1) ? 0f : i / (float)(_ringCount - 1);
                    float radiusAnim = radius + 0.010f * _signed * (0.15f + 0.85f * t);

                    float width = _ringWidth[i] * widthScale;

                    float d = MathF.Abs(rNorm - radiusAnim);
                    if (d > width)
                    {
                        continue;
                    }

                    float local = 1f - d / width;

                    if (_ringSegmented[i] && _ringSegmentsPerWedge[i] > 0)
                    {
                        float segFreq = _ringSegmentsPerWedge[i] * (0.85f + 0.25f * _loop);

                        float segPos = wedgeNorm * segFreq + 0.15f * MathF.Sin(_phase);
                        int segIndex = (int)MathF.Floor(segPos);

                        if ((segIndex & 1) == 0)
                        {
                            local *= 0.28f + 0.10f * _loop2;
                        }
                    }

                    if (local > best)
                    {
                        best = local;
                    }
                }

                return best;
            }

            private float SquaresMask (float dx, float dy, float widthScale)
            {
                float sNorm = MathF.Max(MathF.Abs(dx), MathF.Abs(dy)) / _radiusMax;
                float best = 0f;

                float wobble = 1f + 0.015f * _signed;
                sNorm *= wobble;

                for (int i = 0; i < _squareCount; i++)
                {
                    float radius = _squareRadius[i];
                    float width = _squareWidth[i] * widthScale;

                    float d = MathF.Abs(sNorm - radius);
                    if (d > width)
                    {
                        continue;
                    }

                    float local = 1f - d / width;
                    if (local > best)
                    {
                        best = local;
                    }
                }

                return best;
            }

            private float CrossMask (float dx, float dy, float rNorm, float widthScale)
            {
                float mask = 0f;

                float nx = dx / _radiusMax;
                float ny = dy / _radiusMax;

                float plusW = _plusWidth * widthScale;
                float diagW = _diagWidth * widthScale;

                float plusMax = _plusMaxR + 0.015f * _signed;
                float diagMax = _diagMaxR + 0.015f * _signed;

                if (_hasPlusCross && rNorm < plusMax)
                {
                    float ax = MathF.Abs(nx);
                    float ay = MathF.Abs(ny);

                    if (ax < plusW)
                    {
                        float local = 1f - ax / plusW;
                        if (local > mask)
                        {
                            mask = local;
                        }
                    }

                    if (ay < plusW)
                    {
                        float local = 1f - ay / plusW;
                        if (local > mask)
                        {
                            mask = local;
                        }
                    }
                }

                if (_hasDiagonalCross && rNorm < diagMax)
                {
                    const float invSqrt2 = 0.70710678f;

                    float along1 = (nx + ny) * invSqrt2;
                    float along2 = (nx - ny) * invSqrt2;

                    float d1 = MathF.Abs(along1);
                    float d2 = MathF.Abs(along2);

                    if (d1 < diagW)
                    {
                        float local = 1f - d1 / diagW;
                        if (local > mask)
                        {
                            mask = local;
                        }
                    }

                    if (d2 < diagW)
                    {
                        float local = 1f - d2 / diagW;
                        if (local > mask)
                        {
                            mask = local;
                        }
                    }
                }

                return mask;
            }

            private float RaysMask (float rNorm, float wedgeNorm, float widthScale)
            {
                float innerSkip = _rayInnerSkip * (0.92f + 0.14f * _loop2);
                if (rNorm < innerSkip)
                {
                    return 0f;
                }

                float width = _rayWidth * widthScale;

                float distToBoundary = MathF.Min(wedgeNorm, 1f - wedgeNorm);
                if (distToBoundary > width)
                {
                    return 0f;
                }

                float angular = 1f - distToBoundary / width;

                float t = (rNorm - innerSkip) / (1f - innerSkip);
                if (t < 0f)
                {
                    t = 0f;
                }
                if (t > 1f)
                {
                    t = 1f;
                }

                float radialFade = 0.30f + 0.70f * (1f - t);

                float pulse = 0.82f + 0.18f * _loop;

                return angular * radialFade * pulse;
            }

            private Rgba32 BackgroundColor (float rNorm)
            {
                float halo = MathF.Pow(MathF.Max(1f - rNorm * 1.05f, 0f), 1.6f);

                float rA = 6f + 22f * halo;
                float gA = 8f + 30f * halo;
                float bA = 12f + 52f * halo;

                float rB = 4f + 16f * halo;
                float gB = 10f + 26f * halo;
                float bB = 20f + 70f * halo;

                float mix = _loop;

                float r = rA * (1f - mix) + rB * mix;
                float g = gA * (1f - mix) + gB * mix;
                float b = bA * (1f - mix) + bB * mix;

                float pulse = 0.96f + 0.04f * _loop2;
                r *= pulse;
                g *= pulse;
                b *= pulse;

                return new Rgba32(
                    (byte)MathExtensions.Clamp(r, 0f, 255f),
                    (byte)MathExtensions.Clamp(g, 0f, 255f),
                    (byte)MathExtensions.Clamp(b, 0f, 255f));
            }

            private float GildedNoise (float rNorm, float wedgeNorm)
            {
                float v = rNorm * (40f + 8f * _loop) + wedgeNorm * (11f + 2f * _loop2) + _seed * 0.017f;
                v += 0.6f * MathF.Sin(_phase);

                float s = MathF.Sin(v) + 0.5f * MathF.Sin(2f * v + 1.37f);
                float n = s * 0.5f + 0.5f;

                return n;
            }

            private Rgba32 GoldColor (float rNorm)
            {
                float centreBoost = MathF.Pow(MathF.Max(1f - rNorm * 0.9f, 0f), 1.1f);

                float temp = 0.90f + 0.10f * _loop2;

                float intensity = (0.58f + 0.42f * centreBoost) * (0.88f + 0.22f * _loop);

                float r = 205f * intensity * temp;
                float g = 178f * intensity * (0.92f + 0.08f * (1f - temp));
                float b = (90f * intensity + 35f * (1f - rNorm)) * (0.92f + 0.08f * (1f - temp));

                return new Rgba32(
                    (byte)MathExtensions.Clamp(r, 0f, 255f),
                    (byte)MathExtensions.Clamp(g, 0f, 255f),
                    (byte)MathExtensions.Clamp(b, 0f, 255f));
            }
        }
    }
}