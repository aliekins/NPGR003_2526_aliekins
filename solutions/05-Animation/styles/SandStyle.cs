using System;
using _05Animation.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _05Animation.Styles
{
    public class SandStyle : IMandalaStyle
    {
        public MandalaStyleKind Kind => MandalaStyleKind.Sand;

        public void Render (MandalaConfig config, Image<Rgba32> image, float time)
        {
            var context = new SandContext(config, image, time);
            context.Render();
        }

        private sealed class SandContext
        {
            private readonly MandalaConfig _config;
            private readonly Image<Rgba32> _image;

            private readonly int _width;
            private readonly int _height;
            private readonly int _symmetry;
            private readonly int _symmetry2;

            private readonly float _cx;
            private readonly float _cy;
            private readonly float _radiusMax;

            private readonly float _detail;
            private readonly int _seed;

            private readonly int _bandCount;
            private readonly int[] _segmentsPerWedge;
            private readonly float _wedgeSize;
            private readonly float _wedgeSize2;

            // Original palette (A)
            private static readonly Rgba32[] PaletteA =
            {
                new Rgba32(235,  70,  60),   // red
                new Rgba32(245, 145,  50),   // orange
                new Rgba32(250, 210,  70),   // yellow
                new Rgba32( 90, 180,  90),   // green
                new Rgba32( 80, 145, 210),   // blue
                new Rgba32(190,  90, 190),   // magenta
                new Rgba32(240, 240, 240)    // white-ish
            };

            private static readonly Rgba32[] PaletteB =
            {
                new Rgba32(245, 220, 120),   // gold
                new Rgba32(225, 190,  80),   // darker gold
                new Rgba32(190, 150,  60),   // bronze
                new Rgba32( 35,  60, 140),   // deep blue
                new Rgba32( 20,  45, 110),   // deeper blue
                new Rgba32( 60,  85, 170),   // blue highlight
                new Rgba32(235, 235, 245)    // pale pearl
            };

            private readonly float _phase;
            private readonly float _loop;
            private readonly float _loop2;
            private readonly float _signed;

            private readonly float _driftU0;
            private readonly float _driftV0;

            public SandContext (MandalaConfig config, Image<Rgba32> image, float time)
            {
                _config = config;
                _image = image;

                _width = config.Width;
                _height = config.Height;

                _symmetry = Math.Max(4, config.Symmetry);
                _symmetry2 = _symmetry + 2;

                _cx = _width / 2f;
                _cy = _height / 2f;
                _radiusMax = MathF.Min(_width, _height) / 2f;

                _detail = (float)config.Detail;
                _seed = config.Seed ?? 0;

                _wedgeSize = 2f * MathF.PI / _symmetry;
                _wedgeSize2 = 2f * MathF.PI / _symmetry2;

                _bandCount = 5 + (int)(_detail * 5f);
                _segmentsPerWedge = new int[_bandCount];

                var random = new Random(_seed);
                for (int i = 0; i < _bandCount; i++)
                {
                    int minSeg = 1;
                    int maxSeg = 2 + (int)(_detail * 5f);
                    int baseSeg = minSeg + random.Next(maxSeg - minSeg + 1);

                    float tBand = (i == 0) ? 0f : i / (float)(_bandCount - 1);
                    int extra = tBand > 0.5f ? 1 : 0;

                    _segmentsPerWedge[i] = baseSeg + extra;
                }

                float t = MathExtensions.Clamp01(time);
                _phase = 2f * MathF.PI * t;
                _loop = 0.5f - 0.5f * MathF.Cos(_phase);
                _loop2 = 0.5f - 0.5f * MathF.Cos(2f * _phase);
                _signed = 2f * _loop - 1f;

                _driftU0 = ((random.Next(10000) / 10000f) * 2f * MathF.PI);
                _driftV0 = ((random.Next(10000) / 10000f) * 2f * MathF.PI);
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

                float rPx = MathF.Sqrt(dx * dx + dy * dy);
                float rNorm = rPx / _radiusMax;

                if (rNorm > 1f)
                {
                    return new Rgba32(8, 8, 10);
                }

                float angle = MathF.Atan2(dy, dx);
                if (angle < 0)
                {
                    angle += 2f * MathF.PI;
                }

                float foldedA = angle % _wedgeSize;
                float foldedB = angle % _wedgeSize2;

                float aU = foldedA / _wedgeSize;
                float bU = foldedB / _wedgeSize2;

                float angleInWedgeNorm = aU * (1f - _loop2) + bU * _loop2;

                int bandIndex = GetBandIndex(rNorm);

                float bandInner = bandIndex / (float)_bandCount;
                float bandOuter = (bandIndex + 1) / (float)_bandCount;
                float bandLocalR = (rNorm - bandInner) / (bandOuter - bandInner);

                int segCountBase = _segmentsPerWedge[bandIndex];

                float segFreq = segCountBase * (0.85f + 0.25f * _loop);

                float driftU = 0.12f * MathF.Sin(_phase + _driftU0);
                float driftR = 0.10f * MathF.Sin(_phase + _driftV0);

                float segPos = (angleInWedgeNorm + driftU) * segFreq;
                int segIndex = FloorMod((int)MathF.Floor(segPos), Math.Max(1, segCountBase));
                float segLocalU = segPos - MathF.Floor(segPos);

                float bandLocalR2 = bandLocalR + 0.08f * driftR * (0.25f + 0.75f * rNorm);
                bandLocalR2 = MathExtensions.Clamp01(bandLocalR2);

                Rgba32 color = GetCellColor(bandIndex, segIndex, rNorm, angleInWedgeNorm);

                ApplyGrain(ref color, rNorm, angleInWedgeNorm);
                ApplyBorders(ref color, bandLocalR2, segLocalU, rNorm);

                if (rNorm < 0.05f)
                {
                    float glow = 0.90f + 0.10f * _loop;
                    color = new Rgba32(
                        (byte)MathExtensions.Clamp(250f * glow, 0f, 255f),
                        (byte)MathExtensions.Clamp(250f * glow, 0f, 255f),
                        (byte)MathExtensions.Clamp(255f * glow, 0f, 255f));
                }

                return color;
            }

            private int GetBandIndex (float rNorm)
            {
                int idx = (int)(rNorm * _bandCount);
                if (idx < 0)
                {
                    idx = 0;
                }
                if (idx >= _bandCount)
                {
                    idx = _bandCount - 1;
                }
                return idx;
            }

            private Rgba32 GetCellColor (int bandIndex, int segIndex, float rNorm, float angleInWedgeNorm)
            {
                float offsetA = 0.35f * MathF.Sin(_phase);
                float offsetB = 0.30f * MathF.Sin(_phase + 1.7f);

                int bi = bandIndex + (int)MathF.Floor(offsetA);
                int si = segIndex + (int)MathF.Floor(offsetB);

                float h = MathExtensions.Hash(bi, si, _seed);

                int paletteIndex = (int)(h * PaletteA.Length);
                if (paletteIndex >= PaletteA.Length)
                {
                    paletteIndex = PaletteA.Length - 1;
                }

                float palMix = _loop;

                Rgba32 a = PaletteA[paletteIndex];
                Rgba32 b = PaletteB[paletteIndex];

                float goldBand = MathF.Exp(-18f * (rNorm - 0.65f) * (rNorm - 0.65f));
                float glint = (0.2f + 0.8f * goldBand) * (0.5f + 0.5f * MathF.Sin(_phase + angleInWedgeNorm * 8f));
                glint *= palMix;

                byte rr = (byte)(a.R * (1f - palMix) + b.R * palMix);
                byte gg = (byte)(a.G * (1f - palMix) + b.G * palMix);
                byte bb = (byte)(a.B * (1f - palMix) + b.B * palMix);

                Rgba32 baseColor = new Rgba32(rr, gg, bb);

                float bright = 0.70f + 0.30f * (1f - rNorm);

                bright *= 0.92f + 0.12f * _loop2;
                bright *= 1f + 0.10f * glint;

                return ScaleColor(baseColor, bright);
            }

            private void ApplyGrain (ref Rgba32 color, float rNorm, float angleInWedgeNorm)
            {
                float gU = 1200f * (0.85f + 0.25f * _loop);
                float gV = 2400f * (0.85f + 0.25f * _loop2);

                int u = (int)(rNorm * gU);
                int v = (int)((angleInWedgeNorm + 0.08f * MathF.Sin(_phase)) * gV);

                float g = MathExtensions.Hash(u, v, _seed + 12345);

                float strengthBase = 0.30f + 0.35f * _detail;
                float strength = strengthBase * (0.80f + 0.35f * _loop);

                float factor = 1f + (g - 0.5f) * 2f * strength;

                if (factor < 0.45f)
                {
                    factor = 0.45f;
                }
                if (factor > 1.55f)
                {
                    factor = 1.55f;
                }

                color = ScaleColor(color, factor);
            }

            private void ApplyBorders (ref Rgba32 color, float bandLocalR, float segLocalU, float rNorm)
            {
                float distRadial = bandLocalR < 0.5f ? bandLocalR : 1f - bandLocalR;
                float distAngular = segLocalU < 0.5f ? segLocalU : 1f - segLocalU;

                float edgeThickness = (0.06f + 0.03f * _detail) * (0.85f + 0.35f * _loop2);

                bool onRadial = distRadial < edgeThickness;
                bool onAngular = distAngular < edgeThickness;

                bool nearOuter = rNorm > (0.95f + 0.01f * _signed);

                if (onRadial || onAngular || nearOuter)
                {
                    float borderDark = 0.30f + 0.08f * _loop;
                    color = ScaleColor(color, borderDark);
                }
            }

            private static Rgba32 ScaleColor (Rgba32 baseColor, float factor)
            {
                float r = baseColor.R * factor;
                float g = baseColor.G * factor;
                float b = baseColor.B * factor;

                byte rr = (byte)MathExtensions.Clamp(r, 0f, 255f);
                byte gg = (byte)MathExtensions.Clamp(g, 0f, 255f);
                byte bb = (byte)MathExtensions.Clamp(b, 0f, 255f);

                return new Rgba32(rr, gg, bb);
            }

            private static int FloorMod (int a, int m)
            {
                if (m <= 0)
                {
                    return 0;
                }

                int r = a % m;
                if (r < 0)
                {
                    r += m;
                }
                return r;
            }
        }
    }
}