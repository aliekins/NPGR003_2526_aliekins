using System;
using SixLabors.ImageSharp.PixelFormats;
using _06ImageRecoloring.Color;

namespace _06ImageRecoloring.Transforms
{
    public sealed class HueShiftNonSkinTransform : IMaskedRecolorTransform
    {
        private readonly double _shiftDegrees;
        private readonly bool _debug;

        public HueShiftNonSkinTransform (double shiftDegrees, bool debugMaskMode = false)
        {
            _shiftDegrees = shiftDegrees;
            _debug = debugMaskMode;
        }

        public bool IsDebugMaskMode => _debug;

        public Rgba32 Apply (Rgba32 original, HsvColor hsv, double pSkin)
        {
            if (_debug)
            {
                byte v = (byte)Math.Clamp((int)Math.Round(pSkin * 255.0), 0, 255);
                return new Rgba32(v, v, v, 255);
            }

            double a = 1.0 - Clamp01(pSkin);

            if (a <= 0.0001)
            {
                return original;
            }

            double newHue = WrapHue(hsv.Hue + _shiftDegrees * a);

            HsvColor shifted = hsv.WithHue(newHue);
            Rgba32 shiftedRgb = ColorConverter.ToRgb(shifted, original.A);

            return LerpRgb(original, shiftedRgb, a);
        }

        private static double Clamp01 (double v)
        {
            if (v < 0.0)
                return 0.0;
            if (v > 1.0)
                return 1.0;
            return v;
        }

        private static double WrapHue (double h)
        {
            h %= 360.0;
            if (h < 0.0)
                h += 360.0;
            return h;
        }

        private static Rgba32 LerpRgb (Rgba32 a, Rgba32 b, double t)
        {
            byte r = (byte)Math.Clamp((int)Math.Round(a.R + (b.R - a.R) * t), 0, 255);
            byte g = (byte)Math.Clamp((int)Math.Round(a.G + (b.G - a.G) * t), 0, 255);
            byte bl = (byte)Math.Clamp((int)Math.Round(a.B + (b.B - a.B) * t), 0, 255);
            return new Rgba32(r, g, bl, a.A);
        }
    }
}