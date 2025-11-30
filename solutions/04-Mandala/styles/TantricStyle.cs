using System;
using _04Mandala.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _04Mandala.Styles
{
    public class TantricStyle : IMandalaStyle
    {
        public MandalaStyleKind Kind => MandalaStyleKind.Tantric;

        public void Render (MandalaConfig config, Image<Rgba32> image)
        {
            int width = config.Width;
            int height = config.Height;
            int symmetry = config.Symmetry;

            float cx = width / 2f;
            float cy = height / 2f;
            float radiusMax = MathF.Min(width, height) / 2f;

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

                        int starPoints = symmetry;
                        float starWave = MathF.Cos(angle * starPoints);
                        float starMask = MathF.Abs(starWave);

                        bool inInnerDisc = rNorm < 0.25f + 0.05f * starMask;
                        bool inStarBand  = rNorm >= 0.25f && rNorm < 0.7f && starMask > 0.4f;
                        bool inOuterRing = rNorm >= 0.7f && rNorm < 0.8f;

                        if (inInnerDisc)
                        {
                            row[x] = new Rgba32(230, 210, 90);
                        }
                        else if (inStarBand)
                        {
                            float stripe = MathF.Sin(rNorm * 40f);
                            float mix = stripe > 0 ? 1f : 0.6f;
                            byte rCol = (byte)(200 * mix);
                            byte gCol = (byte)(60  * mix);
                            byte bCol = (byte)(60  * mix);
                            row[x] = new Rgba32(rCol, gCol, bCol);
                        }
                        else if (inOuterRing)
                        {
                            row[x] = new Rgba32(230, 230, 230);
                        }
                        else
                        {
                            byte bg = (byte)(20 + 30 * (1f - rNorm));
                            row[x] = new Rgba32(bg, bg, bg);
                        }
                    }
                }
            });
        }
    }
}