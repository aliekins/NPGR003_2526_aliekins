using System;
using _05Animation.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _05Animation.Styles
{
    public class LotusStyle : IMandalaStyle
    {
        public MandalaStyleKind Kind => MandalaStyleKind.Lotus;

        public void Render (MandalaConfig config, Image<Rgba32> image, float time)
        {
            var ctx = new LotusContext(config, image, time);
            ctx.Render();
        }

        private sealed class LotusContext
        {
            private readonly Image<Rgba32> _image;
            private readonly int _width;
            private readonly int _height;

            private readonly int _symmetry;
            private readonly int _symmetry2;

            private readonly float _cx;
            private readonly float _cy;
            private readonly float _radiusMax;

            private readonly float _detail;

            private readonly float _wedgeSize;
            private readonly float _wedgeHalf;

            private readonly int _layerCount;

            private readonly float[] _rInnerBase;
            private readonly float[] _rOuterBase;

            private readonly float[] _phaseBase;

            private readonly float _phase;
            private readonly float _loop;
            private readonly float _loop2;
            private readonly float _signed;
            private readonly float _rot;

            public LotusContext (MandalaConfig config, Image<Rgba32> image, float time)
            {
                _image = image;
                _width = config.Width;
                _height = config.Height;

                _symmetry = Math.Max(4, config.Symmetry);
                _symmetry2 = _symmetry + 2;

                _cx = _width / 2f;
                _cy = _height / 2f;
                _radiusMax = MathF.Min(_width, _height) / 2f;

                _detail = (float)Math.Clamp(config.Detail, 0.0, 1.0);

                _wedgeSize = 2f * MathF.PI / _symmetry;
                _wedgeHalf = _wedgeSize / 2f;

                _layerCount = 2 + (int)(_detail * 3f);
                if (_layerCount < 2)
                {
                    _layerCount = 2;
                }

                _rInnerBase = new float[_layerCount];
                _rOuterBase = new float[_layerCount];
                _phaseBase = new float[_layerCount];

                float t = MathExtensions.Clamp01(time);
                _phase = 2f * MathF.PI * t;
                _loop = 0.5f - 0.5f * MathF.Cos(_phase);
                _loop2 = 0.5f - 0.5f * MathF.Cos(2f * _phase);
                _signed = 2f * _loop - 1f;
                _rot = 0.18f * _phase;

                InitialiseLayers();
            }

            private void InitialiseLayers ()
            {
                float rStart = 0.18f;
                float rEnd = 0.95f;

                float fullSpan = rEnd - rStart;
                float spacing = fullSpan / _layerCount;
                float widthFactor = 0.7f;

                for (int i = 0; i < _layerCount; i++)
                {
                    float inner = rStart + i * spacing;
                    float outer = inner + spacing * widthFactor;
                    if (outer > rEnd)
                    {
                        outer = rEnd;
                    }

                    _rInnerBase[i] = inner;
                    _rOuterBase[i] = outer;
                    _phaseBase[i] = (i % 2 == 0) ? 0f : (_wedgeSize / 2f);
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

                float r = MathF.Sqrt(dx * dx + dy * dy);
                float rNorm = r / _radiusMax;

                if (rNorm > 1f)
                {
                    return new Rgba32(10, 10, 20);
                }

                float angle = MathF.Atan2(dy, dx);
                if (angle < 0f)
                {
                    angle += 2f * MathF.PI;
                }

                angle += _rot;

                Rgba32 color = BackgroundColor(rNorm);

                float bestMask = 0f;
                int bestLayer = -1;
                float bestT = 0f;

                for (int layer = 0; layer < _layerCount; layer++)
                {
                    float mask;
                    float tRadial;

                    if (TryComputePetalMask(layer, rNorm, angle, out mask, out tRadial) && mask > bestMask)
                    {
                        bestMask = mask;
                        bestLayer = layer;
                        bestT = tRadial;
                    }
                }

                if (bestLayer >= 0 && bestMask > 0f)
                {
                    color = ShadePetal(bestLayer, bestMask, bestT, rNorm, angle);
                }

                if (rNorm < 0.08f)
                {
                    float glow = 0.85f + 0.15f * _loop;
                    color = new Rgba32(
                        (byte)MathExtensions.Clamp(240f * glow, 0f, 255f),
                        (byte)MathExtensions.Clamp(220f * glow, 0f, 255f),
                        (byte)MathExtensions.Clamp(235f * glow, 0f, 255f));
                }

                return color;
            }

            private bool TryComputePetalMask (int layer, float rNorm, float angle, out float mask, out float tRadial)
            {
                float layerT = _layerCount == 1 ? 0f : layer / (float)(_layerCount - 1);
                float inner = _rInnerBase[layer];
                float outer = _rOuterBase[layer];

                float widthBreath = 0.010f + 0.015f * _detail;
                float centerBreath = 0.006f + 0.012f * _detail;

                float w = (0.25f + 0.75f * layerT);

                float innerAnim = inner + centerBreath * _signed * w;
                float outerAnim = outer + (centerBreath * _signed * w + widthBreath * (2f * _loop2 - 1f) * w);

                if (outerAnim <= innerAnim + 0.001f)
                {
                    outerAnim = innerAnim + 0.001f;
                }

                if (rNorm < innerAnim || rNorm > outerAnim)
                {
                    mask = 0f;
                    tRadial = 0f;
                    return false;
                }

                tRadial = (rNorm - innerAnim) / (outerAnim - innerAnim);

                float wedgeA = 2f * MathF.PI / _symmetry;
                float wedgeB = 2f * MathF.PI / _symmetry2;

                float a = angle + _phaseBase[layer];

                float foldedA = a % wedgeA;
                float foldedB = a % wedgeB;

                float halfA = wedgeA / 2f;
                float halfB = wedgeB / 2f;

                float deltaA = MathF.Abs(foldedA - halfA) / halfA;
                float deltaB = MathF.Abs(foldedB - halfB) / halfB;

                float delta = deltaA * (1f - _loop2) + deltaB * _loop2;

                float maxWidthBase = 0.85f;
                float maxWidth = maxWidthBase * (0.82f + 0.30f * _loop);

                float exponent = (1.1f + 1.4f * _detail) * (0.85f + 0.40f * _loop2);

                float allowed = maxWidth * (1f - MathF.Pow(tRadial, exponent));
                if (allowed <= 0f)
                {
                    mask = 0f;
                    return false;
                }

                float ripple = 0.5f + 0.5f * MathF.Sin(2f * MathF.PI * (tRadial * (1.8f + 2.5f * _detail)) + _phase);
                float serration = 1f + (0.10f + 0.18f * _detail) * _loop * (ripple - 0.5f);
                allowed *= serration;

                if (delta > allowed)
                {
                    mask = 0f;
                    return false;
                }

                float angTerm = 1f - (delta / allowed);
                float mid = 0.55f + 0.05f * (2f * _loop2 - 1f);
                float dMid = (tRadial - mid) / 0.55f;
                float radialTerm = 1f - dMid * dMid;

                if (radialTerm < 0f)
                {
                    radialTerm = 0f;
                }

                mask = angTerm * radialTerm;
                if (mask < 0f)
                {
                    mask = 0f;
                }
                if (mask > 1f)
                {
                    mask = 1f;
                }
                return mask > 0f;
            }

            private Rgba32 ShadePetal (int layer, float mask, float tRadial, float rNorm, float angle)
            {
                float layerT = _layerCount == 1 ? 0f : layer / (float)(_layerCount - 1);

                float baseRA = 220f - 40f * layerT;
                float baseGA = 150f + 60f * layerT;
                float baseBA = 200f + 40f * layerT;

                float gold = 245f - 25f * layerT;
                float deepB = 40f + 80f * layerT;

                float baseRB = gold * (0.65f + 0.35f * (1f - layerT));
                float baseGB = 210f * (0.55f + 0.45f * (1f - layerT));
                float baseBB = deepB;

                float palMix = _loop;
                float baseR = baseRA * (1f - palMix) + baseRB * palMix;
                float baseG = baseGA * (1f - palMix) + baseGB * palMix;
                float baseB = baseBA * (1f - palMix) + baseBB * palMix;

                float tipBoost = 0.5f + 0.5f * (1f - tRadial);
                float intensity = 0.35f + 0.65f * mask * tipBoost;

                float shimmer = 0.92f + 0.08f * MathF.Sin(_phase + rNorm * 8f + angle * _symmetry);

                float radialFade = 1f - 0.25f * rNorm;

                float rCol = baseR * intensity * radialFade * shimmer;
                float gCol = baseG * intensity * radialFade * shimmer;
                float bCol = baseB * intensity * radialFade * shimmer;

                byte rr = (byte)MathExtensions.Clamp(rCol, 0f, 255f);
                byte gg = (byte)MathExtensions.Clamp(gCol, 0f, 255f);
                byte bb = (byte)MathExtensions.Clamp(bCol, 0f, 255f);

                return new Rgba32(rr, gg, bb);
            }

            private Rgba32 BackgroundColor (float rNorm)
            {
                byte rA = (byte)(40 + 40 * (1f - rNorm));
                byte gA = (byte)(60 + 40 * (1f - rNorm));
                byte bA = (byte)(90 + 50 * rNorm);

                byte rB = (byte)(10 + 22 * (1f - rNorm));
                byte gB = (byte)(12 + 26 * (1f - rNorm));
                byte bB = (byte)(30 + 120 * rNorm);

                float mix = _loop;

                byte rr = (byte)(rA * (1f - mix) + rB * mix);
                byte gg = (byte)(gA * (1f - mix) + gB * mix);
                byte bb = (byte)(bA * (1f - mix) + bB * mix);

                float pulse = 0.94f + 0.06f * _loop2;

                return new Rgba32(
                    (byte)MathExtensions.Clamp(rr * pulse, 0f, 255f),
                    (byte)MathExtensions.Clamp(gg * pulse, 0f, 255f),
                    (byte)MathExtensions.Clamp(bb * pulse, 0f, 255f));
            }
        }
    }
}