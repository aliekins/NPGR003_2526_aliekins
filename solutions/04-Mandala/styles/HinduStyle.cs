using System;
using _04Mandala.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _04Mandala.Styles
{
    public class HinduStyle : IMandalaStyle
    {
        public MandalaStyleKind Kind => MandalaStyleKind.Hindu;

        public void Render (MandalaConfig config, Image<Rgba32> image)
        {
            int width = config.Width;
            int height = config.Height;
            int symmetry = config.Symmetry;
            double detail = config.Detail;

            float cx = width / 2f;
            float cy = height / 2f;
            float radiusMax = MathF.Min(width, height) / 2f;

            int bands = 3 + (int)(detail * 5);

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
                            row[x] = new Rgba32(15, 5, 15);
                            continue;
                        }

                        float angle = MathF.Atan2(dy, dx);
                        if (angle < 0)
                        {
                            angle += 2f * MathF.PI;
                        }

                        float wedgeSize = 2f * MathF.PI / symmetry;
                        float foldedAngle = angle % wedgeSize;

                        float wedgeCenter = wedgeSize / 2f;
                        float angleDist = MathF.Abs(foldedAngle - wedgeCenter) / wedgeCenter;
                        angleDist = MathExtensions.Clamp(angleDist, 0f, 1f);

                        float petalProfile = 1f - angleDist;
                        petalProfile = petalProfile * petalProfile;

                        float bandPos = rNorm * bands;
                        int bandIndex = Math.Clamp((int)bandPos, 0, bands - 1);
                        float bandFrac = bandPos - bandIndex;

                        bool inPetal = bandFrac < petalProfile;

                        if (inPetal)
                        {
                            bool evenBand = (bandIndex % 2) == 0;
                            byte baseR = (byte)(evenBand ? 255 : 230);
                            byte baseG = (byte)(evenBand ? 160 : 120);
                            byte baseB = (byte)(evenBand ? 40  : 90);

                            float fade = 1f - rNorm * 0.4f;
                            row[x] = new Rgba32(
                                (byte)(baseR * fade),
                                (byte)(baseG * fade),
                                (byte)(baseB * fade));
                        }
                        else
                        {
                            byte rBg = (byte)(40 + 80 * (1f - rNorm));
                            byte gBg = (byte)(20 + 40 * (1f - rNorm));
                            byte bBg = (byte)(40 + 60 * rNorm);
                            row[x] = new Rgba32(rBg, gBg, bBg);
                        }
                    }
                }
            });
        }
    }
}