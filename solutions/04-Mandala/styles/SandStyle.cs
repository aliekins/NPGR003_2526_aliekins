using System;
using _04Mandala.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _04Mandala.Styles
{
    public class SandStyle : IMandalaStyle
    {
        public MandalaStyleKind Kind => MandalaStyleKind.Sand;

        public void Render (MandalaConfig config, Image<Rgba32> image)
        {
            var context = new SandContext(config, image);
            context.Render();
        }

        private sealed class SandContext
        {
            private readonly MandalaConfig _config;
            private readonly Image<Rgba32> _image;

            private readonly int _width;
            private readonly int _height;
            private readonly int _symmetry;

            private readonly float _cx;
            private readonly float _cy;
            private readonly float _radiusMax;

            private readonly float _detail;
            private readonly int _seed;

            private readonly int _bandCount;
            private readonly int[] _segmentsPerWedge;
            private readonly float _wedgeSize;

            private static readonly Rgba32[] Palette =
            {
                new Rgba32(235,  70,  60),   // red
                new Rgba32(245, 145,  50),   // orange
                new Rgba32(250, 210,  70),   // yellow
                new Rgba32( 90, 180,  90),   // green
                new Rgba32( 80, 145, 210),   // blue
                new Rgba32(190,  90, 190),   // magenta
                new Rgba32(240, 240, 240)    // white-ish
            };

            public SandContext (MandalaConfig config, Image<Rgba32> image)
            {
                _config = config;
                _image = image;

                _width = config.Width;
                _height = config.Height;
                _symmetry = Math.Max(4, config.Symmetry);

                _cx = _width / 2f;
                _cy = _height / 2f;
                _radiusMax = MathF.Min(_width, _height) / 2f;

                _detail = (float)config.Detail;
                _seed = config.Seed ?? 0;

                _wedgeSize = 2f * MathF.PI / _symmetry;

                _bandCount = 5 + (int)(_detail * 5f);
                _segmentsPerWedge = new int[_bandCount];

                var random = new Random(_seed);
                for (int i = 0; i < _bandCount; i++)
                {
                    int minSeg = 1;
                    int maxSeg = 2 + (int)(_detail * 5f);
                    int baseSeg = minSeg + random.Next(maxSeg - minSeg + 1);

                    float t = (i == 0) ? 0f : i / (float)(_bandCount - 1);
                    int extra = t > 0.5f ? 1 : 0;

                    _segmentsPerWedge[i] = baseSeg + extra;
                }
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
                    return new Rgba32(8, 8, 8);
                }

                float angle = MathF.Atan2(dy, dx);
                if (angle < 0)
                {
                    angle += 2f * MathF.PI;
                }

                int bandIndex = GetBandIndex(rNorm);
                float bandInner = bandIndex / (float)_bandCount;
                float bandOuter = (bandIndex + 1) / (float)_bandCount;
                float bandLocalR = (rNorm - bandInner) / (bandOuter - bandInner);

                float foldedAngle = angle % _wedgeSize;
                float angleInWedgeNorm = foldedAngle / _wedgeSize;

                int segCount = _segmentsPerWedge[bandIndex];
                float segPos = angleInWedgeNorm * segCount;
                int segIndex = Math.Min(segCount - 1, Math.Max(0, (int)segPos));
                float segLocalU = segPos - segIndex;

                Rgba32 color = GetCellColor(bandIndex, segIndex, rNorm);

                ApplyGrain(ref color, rNorm, angleInWedgeNorm);

                ApplyBorders(ref color, bandLocalR, segLocalU, rNorm);

                if (rNorm < 0.05f)
                {
                    color = new Rgba32(250, 250, 250);
                }

                return color;
            }

            private int GetBandIndex (float rNorm)
            {
                int idx = (int)(rNorm * _bandCount);
                if (idx < 0)
                    idx = 0;
                if (idx >= _bandCount)
                    idx = _bandCount - 1;
                return idx;
            }


            private Rgba32 GetCellColor (int bandIndex, int segIndex, float rNorm)
            {
                float h = MathExtensions.Hash(bandIndex, segIndex, _seed);
                int paletteIndex = (int)(h * Palette.Length);
                if (paletteIndex >= Palette.Length)
                    paletteIndex = Palette.Length - 1;

                Rgba32 baseColor = Palette[paletteIndex];

                float bright = 0.7f + 0.3f * (1f - rNorm);
                return ScaleColor(baseColor, bright);
            }

            private void ApplyGrain (ref Rgba32 color, float rNorm, float angleInWedgeNorm)
            {
                int u = (int)(rNorm * 1200f);
                int v = (int)(angleInWedgeNorm * 2400f);
                float g = MathExtensions.Hash(u, v, _seed + 12345);

                float strength = 0.3f + 0.35f * _detail;
                float factor = 1f + (g - 0.5f) * 2f * strength;

                if (factor < 0.4f)
                    factor = 0.4f;
                if (factor > 1.6f)
                    factor = 1.6f;

                color = ScaleColor(color, factor);
            }

            private void ApplyBorders (ref Rgba32 color, float bandLocalR, float segLocalU, float rNorm)
            {
                float distRadial = bandLocalR < 0.5f ? bandLocalR : 1f - bandLocalR;

                float distAngular = segLocalU < 0.5f ? segLocalU : 1f - segLocalU;

                float edgeThickness = 0.08f;

                bool onRadial = distRadial < edgeThickness;
                bool onAngular = distAngular < edgeThickness;

                bool nearOuter = rNorm > 0.95f;

                if (onRadial || onAngular || nearOuter)
                {
                    color = ScaleColor(color, 0.35f);
                }
            }

            // helpers

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
        }
    }
}