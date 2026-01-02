using System;
using _05Animation.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _05Animation.Styles
{
    public class ChakraStyle : IMandalaStyle
    {
        public MandalaStyleKind Kind => MandalaStyleKind.Chakra;

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

            int rings = 7;

            float t = MathExtensions.Clamp01(time);
            float phase = 2f * MathF.PI * t;

            float loop = 0.5f - 0.5f * MathF.Cos(phase);
            float loop2 = 0.5f - 0.5f * MathF.Cos(2f * phase);
            float signed = 2f * loop - 1f;

            float rot = 0.28f * phase;

            float ringBreath = 0.040f * signed;
            float warpAmp = 0.070f + 0.030f * loop;

            byte[][] paletteA =
            {
                new byte[] { 180,  40,  40 }, // red
                new byte[] { 220, 120,  10 }, // orange
                new byte[] { 220, 210,  60 }, // yellow
                new byte[] {  40, 160,  80 }, // green
                new byte[] {  40,  90, 180 }, // blue
                new byte[] { 120,  60, 190 }, // indigo
                new byte[] { 200, 140, 220 }  // violet
            };

            byte[][] paletteB =
            {
                new byte[] { 200,  60,  80 },
                new byte[] { 240, 140,  40 },
                new byte[] { 240, 230, 120 },
                new byte[] {  60, 190, 120 },
                new byte[] {  60, 120, 220 },
                new byte[] { 150,  90, 220 },
                new byte[] { 230, 170, 240 }
            };

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

                        angle += rot;

                        float wedgeSize = 2f * MathF.PI / symmetry;
                        float foldedAngle = angle % wedgeSize;
                        float normalizedAngle = foldedAngle / wedgeSize;

                        float warp = warpAmp * MathF.Sin(phase + rNorm * 10f);
                        normalizedAngle += warp;
                        normalizedAngle -= MathF.Floor(normalizedAngle);

                        float ringFreq = rings * (0.85f + 0.25f * loop);
                        float ringPos = (rNorm + ringBreath * (0.35f + 0.65f * rNorm)) * ringFreq;

                        int ringIndex = Math.Clamp((int)ringPos, 0, rings - 1);
                        float ringFrac = ringPos - ringIndex;

                        float outline = 0.030f + 0.035f * loop;
                        bool onOutline = ringFrac < outline || ringFrac > 1f - outline;

                        int sym2 = symmetry + 2;
                        float spoke1 = MathF.Abs(MathF.Sin((normalizedAngle + 0.08f * MathF.Sin(phase)) * symmetry * MathF.PI));
                        float spoke2 = MathF.Abs(MathF.Sin((normalizedAngle + 0.10f * MathF.Sin(phase)) * sym2 * MathF.PI));
                        float spokeWave = spoke1 * (1f - loop2) + spoke2 * loop2;

                        float spokeThresh = 0.34f + 0.28f * loop;
                        float spokeMask = spokeWave > spokeThresh ? 1f : 0f;

                        float petalFreq = (2f + symmetry * 0.25f) * (0.85f + 0.30f * loop2);
                        float petal = 0.5f + 0.5f * MathF.Sin(2f * MathF.PI * (normalizedAngle * petalFreq) + phase);
                        float petalMask = SmoothStep(0.35f, 0.65f, petal) * (0.2f + 0.8f * rNorm);

                        var a = paletteA[ringIndex];
                        var b = paletteB[ringIndex];
                        float palMix = loop;

                        byte rCol = (byte)(a[0] * (1f - palMix) + b[0] * palMix);
                        byte gCol = (byte)(a[1] * (1f - palMix) + b[1] * palMix);
                        byte bCol = (byte)(a[2] * (1f - palMix) + b[2] * palMix);

                        float lobe = 0.5f + 0.5f * MathF.Sin(normalizedAngle * symmetry * 2f + 0.6f * phase);
                        float brightness = 0.33f + 0.67f * lobe;

                        brightness *= 0.55f + 0.45f * spokeMask;
                        brightness *= 0.72f + 0.28f * petalMask;

                        if (onOutline)
                        {
                            brightness = 1f;
                        }

                        float centerGlow = SmoothStep(0.30f, 0.00f, rNorm);
                        brightness = MathF.Min(1f, brightness + (0.10f + 0.06f * loop) * centerGlow);

                        row[x] = new Rgba32(
                            (byte)Math.Clamp((int)(rCol * brightness), 0, 255),
                            (byte)Math.Clamp((int)(gCol * brightness), 0, 255),
                            (byte)Math.Clamp((int)(bCol * brightness), 0, 255));
                    }
                }
            });
        }

        private static float SmoothStep (float edge0, float edge1, float x)
        {
            if (edge0 == edge1)
            {
                return x < edge0 ? 0f : 1f;
            }

            float t = (x - edge0) / (edge1 - edge0);
            if (t < 0f)
            {
                t = 0f;
            }
            if (t > 1f)
            {
                t = 1f;
            }

            return t * t * (3f - 2f * t);
        }
    }
}