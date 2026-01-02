using System;
using _05Animation.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _05Animation.Styles
{
    public class TantricStyle : IMandalaStyle
    {
        public MandalaStyleKind Kind => MandalaStyleKind.Tantric;

        public void Render (MandalaConfig config, Image<Rgba32> image, float time)
        {
            int width = config.Width;
            int height = config.Height;

            int symmetry = config.Symmetry;
            if (symmetry < 3)
            {
                symmetry = 3;
            }

            float cx = width / 2f;
            float cy = height / 2f;
            float radiusMax = MathF.Min(width, height) / 2f;

            float t = MathExtensions.Clamp01(time);
            float phase = 2f * MathF.PI * t;
            float loop = 0.5f - 0.5f * MathF.Cos(phase);
            float signed = 2f * loop - 1f;
            float rot = 0.25f * phase;

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < height; y++)
                {
                    var row = accessor.GetRowSpan(y);

                    for (int x = 0; x < width; x++)
                    {
                        float dx = x - cx;
                        float dy = y - cy;

                        float r = MathF.Sqrt(dx * dx + dy * dy);
                        float rNorm = r / radiusMax;

                        if (rNorm > 1f)
                        {
                            row[x] = new Rgba32(5, 5, 5);
                            continue;
                        }

                        float angle = MathF.Atan2(dy, dx);
                        if (angle < 0)
                        {
                            angle += 2f * MathF.PI;
                        }

                        angle += rot;

                        int starPoints = symmetry;
                        float starWave = MathF.Cos(angle * starPoints);
                        float starMask = MathF.Abs(starWave);

                        float innerRadius = 0.25f + 0.04f * signed;
                        float innerWave   = 0.05f + 0.03f * loop;

                        float bandOuter   = 0.70f + 0.05f * signed;
                        float starThresh  = 0.38f + 0.12f * loop;

                        float outerStart  = 0.72f + 0.04f * signed;
                        float outerWidth  = 0.10f + 0.02f * loop;

                        bool inInnerDisc =
                            rNorm < innerRadius + innerWave * starMask;

                        bool inStarBand =
                            rNorm >= innerRadius &&
                            rNorm < bandOuter &&
                            starMask > starThresh;

                        bool inOuterRing =
                            rNorm >= outerStart &&
                            rNorm < outerStart + outerWidth;

                        if (inInnerDisc)
                        {
                            float glow = 0.85f + 0.15f * loop;
                            row[x] = new Rgba32(
                                (byte)(230 * glow),
                                (byte)(210 * glow),
                                (byte)(90 * glow));
                        }
                        else if (inStarBand)
                        {
                            float stripe = MathF.Sin(rNorm * 40f + phase);
                            float mix = stripe > 0 ? 1f : 0.6f;

                            byte rCol = (byte)(200 * mix);
                            byte gCol = (byte)(60  * mix);
                            byte bCol = (byte)(60  * mix);

                            row[x] = new Rgba32(rCol, gCol, bCol);
                        }
                        else if (inOuterRing)
                        {
                            float ringPulse = 0.80f + 0.20f * loop;
                            row[x] = new Rgba32(
                                (byte)(230 * ringPulse),
                                (byte)(230 * ringPulse),
                                (byte)(230 * ringPulse));
                        }
                        else
                        {
                            byte bg = (byte)(20 + 35 * (1f - rNorm));
                            row[x] = new Rgba32(bg, bg, bg);
                        }
                    }
                }
            });
        }
    }
}