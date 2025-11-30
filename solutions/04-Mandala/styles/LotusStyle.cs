using System;
using _04Mandala.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _04Mandala.Styles
{
    public class LotusStyle : IMandalaStyle
    {
        public MandalaStyleKind Kind => MandalaStyleKind.Lotus;

        public void Render (MandalaConfig config, Image<Rgba32> image)
        {
            var ctx = new LotusContext(config, image);
            ctx.Render();
        }

        private sealed class LotusContext
        {
            private readonly Image<Rgba32> _image;
            private readonly int _width;
            private readonly int _height;
            private readonly int _symmetry;

            private readonly float _cx;
            private readonly float _cy;
            private readonly float _radiusMax;

            private readonly float _detail;
            private readonly float _wedgeSize;
            private readonly float _wedgeHalf;

            private readonly int _layerCount;
            private readonly float[] _rInner;
            private readonly float[] _rOuter;
            private readonly float[] _phase;

            public LotusContext (MandalaConfig config, Image<Rgba32> image)
            {
                _image = image;
                _width = config.Width;
                _height = config.Height;
                _symmetry = Math.Max(4, config.Symmetry);

                _cx = _width / 2f;
                _cy = _height / 2f;
                _radiusMax = MathF.Min(_width, _height) / 2f;

                _detail = (float)config.Detail;
                _wedgeSize = 2f * MathF.PI / _symmetry;
                _wedgeHalf = _wedgeSize / 2f;

                _layerCount = 2 + (int)(_detail * 3f);
                if (_layerCount < 2)
                    _layerCount = 2;

                _rInner = new float[_layerCount];
                _rOuter = new float[_layerCount];
                _phase = new float[_layerCount];

                InitialiseLayers();
            }

            private void InitialiseLayers ()
            {
                float rStart = 0.18f;
                float rEnd   = 0.95f;

                float fullSpan = rEnd - rStart;
                float spacing = fullSpan / _layerCount;
                float widthFactor = 0.7f;

                for (int i = 0; i < _layerCount; i++)
                {
                    float inner = rStart + i * spacing;
                    float outer = inner + spacing * widthFactor;
                    if (outer > rEnd)
                        outer = rEnd;

                    _rInner[i] = inner;
                    _rOuter[i] = outer;

                    _phase[i] = (i % 2 == 0) ? 0f : (_wedgeSize / 2f);
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
                    color = ShadePetal(bestLayer, bestMask, bestT, rNorm);
                }

                if (rNorm < 0.08f)
                {
                    color = new Rgba32(240, 220, 235);
                }

                return color;
            }

            private bool TryComputePetalMask (int layer, float rNorm, float angle, out float mask, out float tRadial)
            {
                float inner = _rInner[layer];
                float outer = _rOuter[layer];

                if (rNorm < inner || rNorm > outer)
                {
                    mask = 0f;
                    tRadial = 0f;
                    return false;
                }

                tRadial = (rNorm - inner) / (outer - inner);

                float a = angle + _phase[layer];
                float folded = a % _wedgeSize;
                float delta = MathF.Abs(folded - _wedgeHalf) / _wedgeHalf;

                float maxWidth = 0.85f;
                float exponent = 1.2f + 1.5f * _detail;
                float allowed = maxWidth * (1f - MathF.Pow(tRadial, exponent));
                if (allowed <= 0f)
                {
                    mask = 0f;
                    return false;
                }

                if (delta > allowed)
                {
                    mask = 0f;
                    return false;
                }

                float angTerm = 1f - (delta / allowed);

                float mid = 0.55f;
                float dMid = (tRadial - mid) / 0.55f;
                float radialTerm = 1f - dMid * dMid;
                if (radialTerm < 0f)
                    radialTerm = 0f;

                mask = angTerm * radialTerm;
                if (mask < 0f)
                    mask = 0f;
                if (mask > 1f)
                    mask = 1f;
                return mask > 0f;
            }

            private Rgba32 ShadePetal (int layer, float mask, float tRadial, float rNorm)
            {
                float layerT = _layerCount == 1 ? 0f : layer / (float)(_layerCount - 1);

                float baseR = 220f - 40f * layerT;
                float baseG = 150f + 60f * layerT;
                float baseB = 200f + 40f * layerT;

                float tipBoost = 0.5f + 0.5f * (1f - tRadial);
                float intensity = 0.35f + 0.65f * mask * tipBoost;

                float radialFade = 1f - 0.25f * rNorm;

                float rCol = baseR * intensity * radialFade;
                float gCol = baseG * intensity * radialFade;
                float bCol = baseB * intensity * radialFade;

                byte rr = (byte)MathExtensions.Clamp(rCol, 0f, 255f);
                byte gg = (byte)MathExtensions.Clamp(gCol, 0f, 255f);
                byte bb = (byte)MathExtensions.Clamp(bCol, 0f, 255f);

                return new Rgba32(rr, gg, bb);
            }

            private Rgba32 BackgroundColor (float rNorm)
            {
                byte rBg = (byte)(40 + 40 * (1f - rNorm));
                byte gBg = (byte)(60 + 40 * (1f - rNorm));
                byte bBg = (byte)(90 + 50 * rNorm);
                return new Rgba32(rBg, gBg, bBg);
            }
        }
    }
}