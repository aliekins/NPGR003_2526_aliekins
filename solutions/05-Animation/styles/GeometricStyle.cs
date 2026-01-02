using System;
using _05Animation.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _05Animation.Styles
{
    public class GeometricStyle : IMandalaStyle
    {
        public MandalaStyleKind Kind => MandalaStyleKind.Geometric;

        public void Render (MandalaConfig config, Image<Rgba32> image, float time)
        {
            var context = new GeometricContext(config, image, time);
            context.Render();
        }

        //  Internal helpers
        private readonly struct RadialCurve
        {
            public readonly float BaseRadius;
            public readonly float Amp1;
            public readonly int   Freq1;
            public readonly float Phase1;

            public readonly float Amp2;
            public readonly int   Freq2;
            public readonly float Phase2;

            public RadialCurve (
                float baseRadius,
                float amp1, int freq1, float phase1,
                float amp2, int freq2, float phase2)
            {
                BaseRadius = baseRadius;
                Amp1 = amp1;
                Freq1 = freq1;
                Phase1 = phase1;
                Amp2 = amp2;
                Freq2 = freq2;
                Phase2 = phase2;
            }

            public float RadiusAt (float angle)
            {
                float r = BaseRadius;
                r += Amp1 * MathF.Cos(Freq1 * angle + Phase1);
                r += Amp2 * MathF.Cos(Freq2 * angle + Phase2);
                return r;
            }
        }

        private sealed class GeometricContext
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

            private readonly int _bandCount;
            private readonly RadialCurve[] _curves;
            private readonly float[] _baseRadii;

            private readonly float _outerRingRadius;
            private readonly float _outerRingWidth;
            private readonly float _wedgeSize;

            private readonly float _hueOffset;
            private readonly float _phase;
            private readonly float _loop;
            private readonly float _loop2;
            private readonly float _signed;
            private readonly float _rot;

            public GeometricContext (MandalaConfig config, Image<Rgba32> image, float time)
            {
                _config = config;
                _image = image;

                _width = config.Width;
                _height = config.Height;

                _symmetry = Math.Max(3, config.Symmetry);
                _symmetry2 = _symmetry + 2;

                _cx = _width / 2f;
                _cy = _height / 2f;
                _radiusMax = MathF.Min(_width, _height) / 2f;

                _detail = (float)config.Detail;

                float t = MathExtensions.Clamp01(time);
                _phase = 2f * MathF.PI * t;
                _loop = 0.5f - 0.5f * MathF.Cos(_phase);
                _loop2 = 0.5f - 0.5f * MathF.Cos(2f * _phase);
                _signed = 2f * _loop - 1f;
                _rot = 0.22f * _phase;

                int seed = config.Seed ?? 0;
                var random = new Random(seed);

                _bandCount = 4 + (int)(_detail * 8f);
                int boundaryCount = _bandCount + 1;

                _curves = new RadialCurve[boundaryCount];
                _baseRadii = new float[boundaryCount];

                InitialiseBaseRadii(random, boundaryCount);
                InitialiseCurves(random, boundaryCount);

                _outerRingRadius = 0.97f;
                _outerRingWidth = 0.004f + 0.003f * _detail;

                _wedgeSize = 2f * MathF.PI / _symmetry;

                _hueOffset = (float)random.NextDouble();
            }

            // Public entry 

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

            private void InitialiseBaseRadii (Random random, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    float t = count == 1 ? 0f : i / (float)(count - 1);
                    float baseR = 0.10f + 0.80f * t;
                    float jitter = ((float)random.NextDouble() - 0.5f) * 0.04f;
                    _baseRadii[i] = baseR + jitter;
                }
                Array.Sort(_baseRadii);
            }

            private void InitialiseCurves (Random random, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    float baseR = _baseRadii[i];

                    float strength1 = 0.02f + 0.06f * (float)random.NextDouble();
                    float detailBoost = 0.5f + _detail;
                    float amp1 = strength1 * detailBoost;

                    int mult1 = 1 + random.Next(3);
                    int freq1 = _symmetry * mult1;

                    float phase1 = (float)(random.NextDouble() * 2.0 * Math.PI);

                    float amp2;
                    int freq2;
                    float phase2;

                    if (random.NextDouble() < 0.7)
                    {
                        float strength2 = 0.01f + 0.04f * (float)random.NextDouble();
                        amp2 = strength2 * (0.3f + 0.7f * _detail);

                        int mult2 = 1 + random.Next(5);
                        freq2 = _symmetry * mult2;

                        phase2 = (float)(random.NextDouble() * 2.0 * Math.PI);
                    }
                    else
                    {
                        amp2 = 0f;
                        freq2 = _symmetry;
                        phase2 = 0f;
                    }

                    _curves[i] = new RadialCurve(baseR, amp1, freq1, phase1, amp2, freq2, phase2);
                }
            }

            private Rgba32 RenderPixel (int x, int y)
            {
                float dx = x - _cx;
                float dy = y - _cy;

                float rPixels = MathF.Sqrt(dx * dx + dy * dy);
                float rNorm = rPixels / _radiusMax;

                if (rNorm > 1f)
                {
                    return new Rgba32(8, 8, 10);
                }

                float angle = MathF.Atan2(dy, dx);
                if (angle < 0)
                {
                    angle += 2f * MathF.PI;
                }

                angle += _rot;

                int wedgeIndex = (int)(angle / _wedgeSize);
                int bandIndex = GetBandIndexForRadius(rNorm);

                Rgba32 color = GetBaseColor(rNorm, bandIndex, wedgeIndex);

                OverlayLineWork(rNorm, angle, ref color);

                return color;
            }

            private int GetBandIndexForRadius (float rNorm)
            {
                int idx = 0;
                while (idx + 1 < _baseRadii.Length && rNorm > _baseRadii[idx + 1])
                {
                    idx++;
                }

                if (idx >= _bandCount)
                {
                    idx = _bandCount - 1;
                }

                return Math.Max(0, idx);
            }

            private Rgba32 GetBaseColor (float rNorm, int bandIndex, int wedgeIndex)
            {
                float bandT = bandIndex / (float)Math.Max(1, _bandCount - 1);
                float hueA = _hueOffset + bandT * 0.6f + (wedgeIndex / (float)_symmetry) * 0.4f;
                float hueB = _hueOffset + 0.62f + bandT * 0.35f + (wedgeIndex / (float)_symmetry) * 0.15f;

                hueA += 0.06f * MathF.Sin(_phase);
                hueB += 0.05f * MathF.Sin(_phase + 1.3f);

                float hue = hueA * (1f - _loop) + hueB * _loop;
                hue = MathExtensions.Wrap(hue);

                float saturationA = 0.40f + 0.40f * bandT + 0.20f * _detail;
                float saturationB = 0.55f + 0.30f * bandT + 0.25f * _detail;

                float saturation = saturationA * (1f - _loop) + saturationB * _loop;
                saturation *= 0.90f + 0.15f * _loop2;

                float valueA = 0.35f + 0.65f * (1f - rNorm * 0.9f);
                float valueB = 0.28f + 0.72f * (1f - rNorm * 0.95f);

                float value = valueA * (1f - _loop) + valueB * _loop;
                value *= 0.92f + 0.10f * (0.5f + 0.5f * MathF.Cos(_phase + rNorm * 6f));

                saturation = MathExtensions.Clamp01(saturation);
                value = MathExtensions.Clamp01(value);

                return HsvToRgba(hue, saturation, value);
            }

            //  Line features 

            private void OverlayLineWork (float rNorm, float angle, ref Rgba32 color)
            {
                float angle2 = angle * (_symmetry2 / (float)_symmetry);
                float sampleAngle = angle * (1f - _loop2) + angle2 * _loop2;

                float minDistToCurve = float.MaxValue;
                float curveBreath = 1f + 0.22f * _signed * (0.25f + 0.75f * rNorm);

                for (int i = 0; i < _curves.Length; i++)
                {
                    float rCurve = _curves[i].RadiusAt(sampleAngle);
                    float boundaryWeight = i / (float)Math.Max(1, _curves.Length - 1);
                    rCurve += 0.010f * _signed * boundaryWeight;

                    float dist = MathF.Abs(rNorm - rCurve) / curveBreath;

                    if (dist < minDistToCurve)
                    {
                        minDistToCurve = dist;
                    }
                }

                float lineThickness = (0.0024f + 0.0026f * _detail) * (0.85f + 0.35f * _loop);

                float distOuter = MathF.Abs(rNorm - _outerRingRadius);
                float outerWidth = _outerRingWidth * (0.85f + 0.40f * _loop);

                float foldedAngle = angle % _wedgeSize;
                float distToSpoke = MathF.Min(foldedAngle, _wedgeSize - foldedAngle);

                float spokeWidthA = _wedgeSize * (0.028f + 0.030f * _detail);
                float spokeWidthB = _wedgeSize * (0.018f + 0.022f * _detail);

                float spokeWidth = spokeWidthA * (1f - _loop) + spokeWidthB * _loop;
                spokeWidth *= 0.85f + 0.30f * _loop2;

                bool onCurve = minDistToCurve < lineThickness;
                bool onOuterRing = distOuter < outerWidth;
                bool onSpoke = distToSpoke < spokeWidth && rNorm > 0.06f;

                float folded2 = angle % (2f * MathF.PI / _symmetry2);
                float wedge2 = 2f * MathF.PI / _symmetry2;
                float distToSpoke2 = MathF.Min(folded2, wedge2 - folded2);
                bool onSpoke2 = distToSpoke2 < spokeWidth * 0.75f && rNorm > 0.10f;

                bool onAnySpoke = onSpoke || (onSpoke2 && _loop2 > 0.15f);

                if (onCurve || onOuterRing || onAnySpoke)
                {
                    color = new Rgba32(10, 10, 10);
                }

                if (rNorm < 0.04f)
                {
                    color = new Rgba32(10, 10, 10);
                }
            }

            //  Math helpers 
            private static Rgba32 HsvToRgba (float h, float s, float v)
            {
                h = MathExtensions.Wrap(h);
                s = MathExtensions.Clamp01(s);
                v = MathExtensions.Clamp01(v);

                float c = v * s;
                float hp = h * 6f;
                float x = c * (1f - MathF.Abs((hp % 2f) - 1f));

                float r1, g1, b1;
                if (hp < 1f)
                { r1 = c; g1 = x; b1 = 0f; }
                else if (hp < 2f)
                { r1 = x; g1 = c; b1 = 0f; }
                else if (hp < 3f)
                { r1 = 0f; g1 = c; b1 = x; }
                else if (hp < 4f)
                { r1 = 0f; g1 = x; b1 = c; }
                else if (hp < 5f)
                { r1 = x; g1 = 0f; b1 = c; }
                else
                { r1 = c; g1 = 0f; b1 = x; }

                float m = v - c;
                byte r = (byte)(MathExtensions.Clamp01(r1 + m) * 255f);
                byte g = (byte)(MathExtensions.Clamp01(g1 + m) * 255f);
                byte b = (byte)(MathExtensions.Clamp01(b1 + m) * 255f);

                return new Rgba32(r, g, b);
            }
        }
    }
}