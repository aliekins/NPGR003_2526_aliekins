using System;
using _05Animation.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _05Animation.Styles
{
    public class CelticStyle : IMandalaStyle
    {
        public MandalaStyleKind Kind => MandalaStyleKind.Celtic;

        public void Render (MandalaConfig config, Image<Rgba32> image, float time)
        {
            var ctx = new CelticContext(config, image, time);
            ctx.Render();
        }

        public sealed class CelticContext
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
            private readonly int _seed;

            private readonly float _wedgeSize;
            private readonly float _wedgeSize2;

            private readonly float _radialTilesBase;
            private readonly float _angularTilesPerWedgeBase;

            private readonly float _tilePhaseU;
            private readonly float _tilePhaseV;

            private readonly float _strandHalfWidthBase;
            private readonly float _borderFracBase;
            private readonly float _waveAmpBase;

            private readonly float _phaseVert1;
            private readonly float _phaseVert2;
            private readonly float _phaseHorz1;
            private readonly float _phaseHorz2;

            private readonly float _phase;
            private readonly float _loop;
            private readonly float _loop2;
            private readonly float _signed;

            public CelticContext (MandalaConfig config, Image<Rgba32> image, float time)
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
                _seed = config.Seed ?? 0;

                _wedgeSize = 2f * MathF.PI / _symmetry;
                _wedgeSize2 = 2f * MathF.PI / _symmetry2;

                _radialTilesBase = 4f + _detail * 6f;
                _angularTilesPerWedgeBase = 4f + _detail * 6f;

                _tilePhaseU = ((_seed & 0xFF) / 255f);
                _tilePhaseV = (((_seed >> 8) & 0xFF) / 255f);

                _strandHalfWidthBase = 0.10f;
                _borderFracBase = 0.35f;

                _waveAmpBase = 0.13f + 0.05f * _detail;

                var rng = new Random(_seed);
                _phaseVert1 = (float)(rng.NextDouble() * 2.0 * Math.PI);
                _phaseVert2 = (float)(rng.NextDouble() * 2.0 * Math.PI);
                _phaseHorz1 = (float)(rng.NextDouble() * 2.0 * Math.PI);
                _phaseHorz2 = (float)(rng.NextDouble() * 2.0 * Math.PI);

                float t = MathExtensions.Clamp01(time);
                _phase = 2f * MathF.PI * t;
                _loop = 0.5f - 0.5f * MathF.Cos(_phase);
                _loop2 = 0.5f - 0.5f * MathF.Cos(2f * _phase);
                _signed = 2f * _loop - 1f;
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
                    return new Rgba32(245, 245, 248);
                }

                float angle = MathF.Atan2(dy, dx);
                if (angle < 0)
                {
                    angle += 2f * MathF.PI;
                }

                float foldedA = angle % _wedgeSize;
                float foldedB = angle % _wedgeSize2;

                float wedgeU_A = foldedA / _wedgeSize;
                float wedgeU_B = foldedB / _wedgeSize2;

                float wedgeU = wedgeU_A * (1f - _loop2) + wedgeU_B * _loop2;

                var tile = MapToTile(rNorm, wedgeU);

                float brightness = ComputeStrandBrightness(tile, rNorm);

                brightness = ApplyOuterRing(brightness, rNorm);
                brightness = ApplyCentre(brightness, rNorm);

                return ShadeCeltic(brightness, rNorm, wedgeU);
            }

            private readonly struct TileCoord
            {
                public readonly int UIndex;
                public readonly int VIndex;
                public readonly float X;
                public readonly float Y;

                public TileCoord (int uIndex, int vIndex, float x, float y)
                {
                    UIndex = uIndex;
                    VIndex = vIndex;
                    X = x;
                    Y = y;
                }
            }

            private TileCoord MapToTile (float rNorm, float wedgeU)
            {
                float radialTiles = _radialTilesBase * (0.85f + 0.25f * _loop);
                float angularTiles = _angularTilesPerWedgeBase * (0.85f + 0.25f * _loop2);

                float driftU = 0.18f * MathF.Sin(_phase);
                float driftV = 0.16f * MathF.Sin(_phase + 1.3f);

                float u = rNorm * radialTiles + _tilePhaseU + driftU;
                float v = wedgeU * angularTiles + _tilePhaseV + driftV;

                int uIdx = (int)MathF.Floor(u);
                int vIdx = (int)MathF.Floor(v);

                float x = v - vIdx;
                float y = u - uIdx;

                return new TileCoord(uIdx, vIdx, x, y);
            }

            private float ComputeStrandBrightness (TileCoord t, float rNorm)
            {
                float x = t.X;
                float y = t.Y;

                float s2pi = 2f * MathF.PI;

                float strandHalfWidth = _strandHalfWidthBase * (0.82f + 0.28f * _loop);
                float borderFrac = _borderFracBase * (0.85f + 0.25f * _loop2);
                float waveAmp = _waveAmpBase * (0.80f + 0.30f * _loop);

                float flowY = 0.85f * _phase;
                float flowX = 0.70f * _phase;

                float vert1Pos = 0.25f + waveAmp * MathF.Sin(s2pi * y + _phaseVert1 + flowY);
                float vert2Pos = 0.75f + waveAmp * MathF.Sin(s2pi * (1f - y) + _phaseVert2 - 0.8f * flowY);

                float horz1Pos = 0.25f + waveAmp * MathF.Sin(s2pi * x + _phaseHorz1 + flowX);
                float horz2Pos = 0.75f + waveAmp * MathF.Sin(s2pi * (1f - x) + _phaseHorz2 - 0.75f * flowX);

                float dv1 = MathF.Abs(x - vert1Pos);
                float dv2 = MathF.Abs(x - vert2Pos);
                float dh1 = MathF.Abs(y - horz1Pos);
                float dh2 = MathF.Abs(y - horz2Pos);

                float dv = MathF.Min(dv1, dv2);
                float dh = MathF.Min(dh1, dh2);

                bool inVertical = dv < strandHalfWidth;
                bool inHorizontal = dh < strandHalfWidth;

                float brightness = 0.97f;

                if (!inVertical && !inHorizontal)
                {
                    float fade = 0.88f + 0.12f * (1f - rNorm);
                    return brightness * fade;
                }

                int parity = (t.UIndex + t.VIndex) & 1;

                if (inVertical && inHorizontal)
                {
                    bool verticalOver = parity == 0;
                    brightness = CrossingBrightness(dv, dh, verticalOver, strandHalfWidth);
                }
                else
                {
                    float d = inVertical ? dv : dh;
                    brightness = SingleStrandBrightness(d, strandHalfWidth);
                }

                brightness = ApplyStrandBorder(brightness, inVertical, inHorizontal, dv, dh, strandHalfWidth, borderFrac);

                return brightness;
            }

            private float SingleStrandBrightness (float distToCenter, float strandHalfWidth)
            {
                float t = distToCenter / strandHalfWidth;
                float core = 0.30f + 0.70f * (1f - t * 0.8f);
                return core;
            }

            private float CrossingBrightness (float dv, float dh, bool verticalOver, float strandHalfWidth)
            {
                float tVert = dv / strandHalfWidth;
                float tHorz = dh / strandHalfWidth;

                float coreVert = 0.30f + 0.70f * (1f - tVert * 0.8f);
                float coreHorz = 0.30f + 0.70f * (1f - tHorz * 0.8f);

                float over = verticalOver ? coreVert : coreHorz;
                float under = verticalOver ? coreHorz * 0.9f : coreVert * 0.9f;

                float mixed = 0.62f * over + 0.38f * under;
                return MathExtensions.Clamp01(mixed);
            }

            private float ApplyStrandBorder (float brightness, bool inVertical, bool inHorizontal, float dv, float dh, float strandHalfWidth, float borderFrac)
            {
                float edgeDist = 1f;

                if (inVertical)
                {
                    float dEdgeV = MathF.Abs(dv - strandHalfWidth) / strandHalfWidth;
                    if (dEdgeV < edgeDist)
                    {
                        edgeDist = dEdgeV;
                    }
                }

                if (inHorizontal)
                {
                    float dEdgeH = MathF.Abs(dh - strandHalfWidth) / strandHalfWidth;
                    if (dEdgeH < edgeDist)
                    {
                        edgeDist = dEdgeH;
                    }
                }

                if (edgeDist < borderFrac)
                {
                    float t = edgeDist / borderFrac;
                    float edgeScale = 0.45f + 0.55f * t;
                    brightness *= edgeScale;
                }

                return brightness;
            }

            private float ApplyOuterRing (float brightness, float rNorm)
            {
                float start = 0.94f + 0.01f * _signed;
                float span = 0.06f + 0.01f * _loop2;

                if (rNorm <= start || rNorm > 1f)
                {
                    return brightness;
                }

                float t = (rNorm - start) / span;
                if (t < 0f)
                    t = 0f;
                if (t > 1f)
                    t = 1f;

                float borderBright = 0.30f + 0.50f * (1f - t);
                return Math.Min(brightness, borderBright);
            }

            private float ApplyCentre (float brightness, float rNorm)
            {
                float centre = 0.05f + 0.01f * _loop;
                if (rNorm < centre)
                {
                    return 0.98f;
                }
                return brightness;
            }

            private Rgba32 ShadeCeltic (float brightness, float rNorm, float wedgeU)
            {
                brightness = MathExtensions.Clamp01(brightness);

                float mix = _loop;

                float paper = 0.92f + 0.06f * (1f - rNorm);
                float deepBlue = 0.18f + 0.22f * rNorm;

                float ink = 1f - brightness;

                float goldBand = MathF.Exp(-16f * (rNorm - 0.65f) * (rNorm - 0.65f));
                float gold = (0.10f + 0.90f * goldBand) * (0.20f + 0.80f * (0.5f + 0.5f * MathF.Sin(_phase + wedgeU * 6f)));
                gold *= mix;

                float rA = paper * (1f - 0.85f * ink);
                float gA = paper * (1f - 0.88f * ink);
                float bA = paper * (1f - 0.90f * ink);

                float rB = deepBlue * (1f - 0.45f * ink) + 0.85f * gold;
                float gB = (deepBlue * 0.85f) * (1f - 0.40f * ink) + 0.72f * gold;
                float bB = (deepBlue * 1.65f) * (1f - 0.30f * ink) + 0.25f * gold;

                float r = rA * (1f - mix) + rB * mix;
                float g = gA * (1f - mix) + gB * mix;
                float b = bA * (1f - mix) + bB * mix;

                float pulse = 0.96f + 0.04f * _loop2;
                r *= pulse;
                g *= pulse;
                b *= pulse;

                return new Rgba32(
                    (byte)MathExtensions.Clamp(r * 255f, 0f, 255f),
                    (byte)MathExtensions.Clamp(g * 255f, 0f, 255f),
                    (byte)MathExtensions.Clamp(b * 255f, 0f, 255f));
            }
        }
    }
}