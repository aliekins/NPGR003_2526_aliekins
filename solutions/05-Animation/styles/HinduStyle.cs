using System;
using _05Animation.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _05Animation.Styles
{
    public class HinduStyle : IMandalaStyle
    {
        public MandalaStyleKind Kind => MandalaStyleKind.Hindu;

        public void Render (MandalaConfig config, Image<Rgba32> image, float time)
        {
            int width = config.Width;
            int height = config.Height;

            int symmetry = config.Symmetry;
            if (symmetry < 3)
            {
                symmetry = 3;
            }

            float detail = (float)Math.Clamp(config.Detail, 0.0, 1.0);

            float cx = width / 2f;
            float cy = height / 2f;
            float radiusMax = MathF.Min(width, height) / 2f;

            int bands = 3 + (int)(detail * 5);

            float t = MathExtensions.Clamp01(time);
            float phase = 2f * MathF.PI * t;

            float loop = 0.5f - 0.5f * MathF.Cos(phase);
            float loop2 = 0.5f - 0.5f * MathF.Cos(2f * phase);
            float signed = 2f * loop - 1f;

            float rot = 0.22f * phase;

            float bandBreath = 0.10f * signed;
            float petalOpen = 0.80f + 0.55f * loop;
            float petalScale = 0.92f + 0.22f * loop;

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
                            row[x] = new Rgba32(10, 6, 14);
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

                        float wedgeCenter = wedgeSize / 2f;
                        float angleDist = MathF.Abs(foldedAngle - wedgeCenter) / wedgeCenter;
                        angleDist = MathExtensions.Clamp(angleDist, 0f, 1f);

                        float petalProfile = 1f - angleDist;
                        petalProfile = petalProfile * petalProfile;
                        petalProfile = MathF.Pow(petalProfile, petalOpen);
                        petalProfile *= petalScale;

                        float wedgeNorm = foldedAngle / wedgeSize;

                        float ripple1 = 0.5f + 0.5f * MathF.Sin(2f * MathF.PI * (wedgeNorm * 2f) + phase);
                        float ripple2 = 0.5f + 0.5f * MathF.Sin(2f * MathF.PI * (wedgeNorm * 3f) + phase);
                        float ripple = ripple1 * (1f - loop2) + ripple2 * loop2;

                        float serration = (0.85f + 0.30f * ripple) * (0.35f + 0.65f * rNorm);
                        petalProfile *= 0.88f + 0.22f * loop * serration;

                        float bandFreq = bands * (0.85f + 0.25f * loop);
                        float bandPos = rNorm * bandFreq + bandBreath * (0.25f + 0.75f * rNorm);

                        if (bandPos < 0f)
                        {
                            bandPos = 0f;
                        }

                        int bandIndex = Math.Clamp((int)bandPos, 0, bands - 1);
                        float bandFrac = bandPos - bandIndex;

                        bool inPetal = bandFrac < petalProfile;

                        if (inPetal)
                        {
                            bool evenBand = (bandIndex % 2) == 0;

                            byte rA = (byte)(evenBand ? 255 : 230);
                            byte gA = (byte)(evenBand ? 160 : 120);
                            byte bA = (byte)(evenBand ? 40  : 90);

                            byte rB = (byte)(evenBand ? 245 : 30);
                            byte gB = (byte)(evenBand ? 215 : 55);
                            byte bB = (byte)(evenBand ? 110 : 120);

                            float palMix = loop;

                            byte baseR = (byte)(rA * (1f - palMix) + rB * palMix);
                            byte baseG = (byte)(gA * (1f - palMix) + gB * palMix);
                            byte baseB = (byte)(bA * (1f - palMix) + bB * palMix);

                            float shimmer = 0.92f + 0.08f * MathF.Sin(phase + rNorm * 8f + foldedAngle * symmetry);

                            float glint = 0.90f + 0.10f * loop2;

                            float fade = (1f - rNorm * 0.4f) * shimmer * glint;

                            row[x] = new Rgba32(
                                (byte)Math.Clamp((int)(baseR * fade), 0, 255),
                                (byte)Math.Clamp((int)(baseG * fade), 0, 255),
                                (byte)Math.Clamp((int)(baseB * fade), 0, 255));
                        }
                        else
                        {
                            byte rBgA = (byte)(40 + 80 * (1f - rNorm));
                            byte gBgA = (byte)(20 + 40 * (1f - rNorm));
                            byte bBgA = (byte)(40 + 60 * rNorm);

                            byte rBgB = (byte)(10 + 25 * (1f - rNorm));
                            byte gBgB = (byte)(12 + 35 * (1f - rNorm));
                            byte bBgB = (byte)(30 + 110 * rNorm);

                            float palMix = loop;

                            byte rBg = (byte)(rBgA * (1f - palMix) + rBgB * palMix);
                            byte gBg = (byte)(gBgA * (1f - palMix) + gBgB * palMix);
                            byte bBg = (byte)(bBgA * (1f - palMix) + bBgB * palMix);

                            float bgPulse = 0.94f + 0.06f * loop;
                            row[x] = new Rgba32(
                                (byte)Math.Clamp((int)(rBg * bgPulse), 0, 255),
                                (byte)Math.Clamp((int)(gBg * bgPulse), 0, 255),
                                (byte)Math.Clamp((int)(bBg * bgPulse), 0, 255));
                        }
                    }
                }
            });
        }
    }
}