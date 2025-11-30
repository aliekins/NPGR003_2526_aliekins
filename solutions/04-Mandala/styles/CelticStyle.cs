using System;
using _04Mandala.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _04Mandala.Styles
{
    public class CelticStyle : IMandalaStyle
    {
        public MandalaStyleKind Kind => MandalaStyleKind.Celtic;

        public void Render (MandalaConfig config, Image<Rgba32> image)
        {
            var ctx = new CelticContext(config, image);
            ctx.Render();
        }

        public sealed class CelticContext
        {
            private readonly MandalaConfig _config;
            private readonly Image<Rgba32> _image;

            private readonly int _width;
            private readonly int _height;
            private readonly int _symmetry;

            private readonly float _cx;
            private readonly float _cy;
            private readonly float _radiusMax;

            private readonly float _detail;
            private readonly int _seed;

            private readonly float _radialTiles;
            private readonly float _angularTilesPerWedge;
            private readonly float _wedgeSize;
            private readonly float _tilePhaseU;
            private readonly float _tilePhaseV;

            private readonly float _strandHalfWidth;
            private readonly float _borderFrac;
            private readonly float _waveAmp;

            private readonly float _phaseVert1;
            private readonly float _phaseVert2;
            private readonly float _phaseHorz1;
            private readonly float _phaseHorz2;

            public CelticContext (MandalaConfig config, Image<Rgba32> image)
            {
                _config = config;
                _image = image;

                _width = config.Width;
                _height = config.Height;
                _symmetry = Math.Max(3, config.Symmetry);

                _cx = _width / 2f;
                _cy = _height / 2f;
                _radiusMax = MathF.Min(_width, _height) / 2f;

                _detail = (float)config.Detail;
                _seed = config.Seed ?? 0;

                _wedgeSize = 2f * MathF.PI / _symmetry;

                _radialTiles = 4f + _detail * 6f;
                _angularTilesPerWedge = 4f + _detail * 6f;

                _tilePhaseU = ((_seed & 0xFF) / 255f);
                _tilePhaseV = (((_seed >> 8) & 0xFF) / 255f);

                _strandHalfWidth = 0.10f;
                _borderFrac = 0.35f;

                _waveAmp = 0.13f + 0.05f * _detail;

                var rng = new Random(_seed);
                _phaseVert1 = (float)(rng.NextDouble() * 2.0 * Math.PI);
                _phaseVert2 = (float)(rng.NextDouble() * 2.0 * Math.PI);
                _phaseHorz1 = (float)(rng.NextDouble() * 2.0 * Math.PI);
                _phaseHorz2 = (float)(rng.NextDouble() * 2.0 * Math.PI);
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
                    return new Rgba32(250, 250, 250);
                }

                float angle = MathF.Atan2(dy, dx);
                if (angle < 0)
                {
                    angle += 2f * MathF.PI;
                }

                float foldedAngle = angle % _wedgeSize;
                float wedgeU = foldedAngle / _wedgeSize;

                var tile = MapToTile(rNorm, wedgeU);

                float brightness = ComputeStrandBrightness(tile, rNorm);

                brightness = ApplyOuterRing(brightness, rNorm);
                brightness = ApplyCentre(brightness, rNorm);

                brightness = MathExtensions.Clamp01(brightness);
                byte v = (byte)(brightness * 255f);
                return new Rgba32(v, v, v);
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
                float u = rNorm * _radialTiles + _tilePhaseU;
                float v = wedgeU * _angularTilesPerWedge + _tilePhaseV;

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

                float vert1Pos = 0.25f + _waveAmp * MathF.Sin(s2pi * y + _phaseVert1);
                float vert2Pos = 0.75f + _waveAmp * MathF.Sin(s2pi * (1f - y) + _phaseVert2);

                float horz1Pos = 0.25f + _waveAmp * MathF.Sin(s2pi * x + _phaseHorz1);
                float horz2Pos = 0.75f + _waveAmp * MathF.Sin(s2pi * (1f - x) + _phaseHorz2);

                float dv1 = MathF.Abs(x - vert1Pos);
                float dv2 = MathF.Abs(x - vert2Pos);
                float dh1 = MathF.Abs(y - horz1Pos);
                float dh2 = MathF.Abs(y - horz2Pos);

                float dv = MathF.Min(dv1, dv2);
                float dh = MathF.Min(dh1, dh2);

                bool inVertical = dv < _strandHalfWidth;
                bool inHorizontal = dh < _strandHalfWidth;

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
                    brightness = CrossingBrightness(dv, dh, verticalOver);
                }
                else
                {
                    float d = inVertical ? dv : dh;
                    brightness = SingleStrandBrightness(d);
                }

                brightness = ApplyStrandBorder(brightness, inVertical, inHorizontal, dv, dh);

                return brightness;
            }

            private float SingleStrandBrightness (float distToCenter)
            {
                float t = distToCenter / _strandHalfWidth;
                float core = 0.30f + 0.70f * (1f - t * 0.8f);
                return core;
            }

            private float CrossingBrightness (float dv, float dh, bool verticalOver)
            {
                float tVert = dv / _strandHalfWidth;
                float tHorz = dh / _strandHalfWidth;

                float coreVert = 0.30f + 0.70f * (1f - tVert * 0.8f);
                float coreHorz = 0.30f + 0.70f * (1f - tHorz * 0.8f);

                float over = verticalOver ? coreVert : coreHorz;
                float under = verticalOver ? coreHorz * 0.9f : coreVert * 0.9f;

                float mixed = 0.62f * over + 0.38f * under;
                return MathExtensions.Clamp01(mixed);
            }

            private float ApplyStrandBorder (float brightness, bool inVertical, bool inHorizontal, float dv, float dh)
            {
                float edgeDist = 1f;

                if (inVertical)
                {
                    float dEdgeV = MathF.Abs(dv - _strandHalfWidth) / _strandHalfWidth;
                    if (dEdgeV < edgeDist)
                        edgeDist = dEdgeV;
                }

                if (inHorizontal)
                {
                    float dEdgeH = MathF.Abs(dh - _strandHalfWidth) / _strandHalfWidth;
                    if (dEdgeH < edgeDist)
                        edgeDist = dEdgeH;
                }

                if (edgeDist < _borderFrac)
                {
                    float t = edgeDist / _borderFrac;
                    float edgeScale = 0.45f + 0.55f * t;
                    brightness *= edgeScale;
                }

                return brightness;
            }

            private float ApplyOuterRing (float brightness, float rNorm)
            {
                if (rNorm <= 0.94f || rNorm > 1f)
                    return brightness;

                float t = (rNorm - 0.94f) / 0.06f;
                float borderBright = 0.30f + 0.50f * (1f - t);
                return Math.Min(brightness, borderBright);
            }

            private float ApplyCentre (float brightness, float rNorm)
            {
                if (rNorm < 0.05f)
                    return 0.98f;
                return brightness;
            }
        }
    }
}