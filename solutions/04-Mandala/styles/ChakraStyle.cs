using System;
using _04Mandala.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _04Mandala.Styles
{
    public class ChakraStyle : IMandalaStyle
    {
        public MandalaStyleKind Kind => MandalaStyleKind.Chakra;

        public void Render (MandalaConfig config, Image<Rgba32> image)
        {
            int width = config.Width;
            int height = config.Height;
            int symmetry = config.Symmetry;

            float cx = width / 2f;
            float cy = height / 2f;
            float radiusMax = MathF.Min(width, height) / 2f;

            int rings = 7;

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
                            row[x] = new Rgba32(5, 5, 15);
                            continue;
                        }

                        float angle = MathF.Atan2(dy, dx);
                        if (angle < 0)
                        {
                            angle += 2f * MathF.PI;
                        }

                        float wedgeSize = 2f * MathF.PI / symmetry;
                        float foldedAngle = angle % wedgeSize;
                        float normalizedAngle = foldedAngle / wedgeSize;

                        float ringPos = rNorm * rings;
                        int ringIndex = Math.Clamp((int)ringPos, 0, rings - 1);
                        float ringFrac = ringPos - ringIndex;

                        bool onOutline = ringFrac < 0.05f || ringFrac > 0.95f;

                        byte[][] palette =
                        {
                            new byte[] { 180, 40, 40 }, // red
                            new byte[] { 220,120, 10 }, // orange
                            new byte[] { 220,210, 60 }, // yellow
                            new byte[] { 40 ,160, 80 }, // green
                            new byte[] { 40 , 90,180 }, // blue
                            new byte[] {120 , 60,190 }, // indigo
                            new byte[] {200 ,140,220 }  // violet
                        };

                        var p = palette[ringIndex];
                        byte rCol = p[0];
                        byte gCol = p[1];
                        byte bCol = p[2];

                        float lobe = 0.5f + 0.5f * MathF.Sin(normalizedAngle * symmetry * 2f);
                        float brightness = 0.4f + 0.6f * lobe;

                        if (onOutline)
                        {
                            brightness = 1f;
                        }

                        row[x] = new Rgba32(
                            (byte)(rCol * brightness),
                            (byte)(gCol * brightness),
                            (byte)(bCol * brightness));
                    }
                }
            });
        }
    }
}