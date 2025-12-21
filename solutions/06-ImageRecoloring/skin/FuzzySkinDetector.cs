using System;
using SixLabors.ImageSharp.PixelFormats;
using _06ImageRecoloring.Color;

namespace _06ImageRecoloring.Skin
{
    public sealed class FuzzySkinDetector : ISkinDetector
    {
        private readonly SkinModel _model;

        public FuzzySkinDetector (SkinModel model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public double SkinProbability (Rgba32 px, HsvColor hsv)
        {
            ColorConverter.ToYCbCr(px, out double y, out double cb, out double cr);

            double dCb = (cb - _model.MeanCb) / _model.StdCb;
            double dCr = (cr - _model.MeanCr) / _model.StdCr;
            double dist2 = dCb * dCb + dCr * dCr;

            // Gaussian pdf without normalization
            double pCbCr = Math.Exp(-0.5 * dist2);

            // Y: fade in from darker values 
            double yScore = SmoothGateLow(y, _model.MinY, softness: 18.0);

            // Cb/Cr
            double cbScore = SmoothGateRange(cb, _model.MinCb, _model.MaxCb, softness: 10.0);
            double crScore = SmoothGateRange(cr, _model.MinCr, _model.MaxCr, softness: 10.0);

            // HSV:
            double sScore = SmoothGateRange(hsv.Saturation, _model.MinS, _model.MaxS, softness: 0.06);
            double vScore = SmoothGateRange(hsv.Value, _model.MinV, _model.MaxV, softness: 0.08);
            double hScore = HueSkinMembership(hsv.Hue, softnessDeg: 8.0);

            double p = pCbCr * yScore * cbScore * crScore * sScore * vScore * hScore;

            if (hsv.Saturation > 0.60)
            {
                double t = ColorConverter.SmoothStep(0.60, 0.85, hsv.Saturation);
                p *= (1.0 - 0.45 * t);
            }

            if (hsv.Value < 0.22)
            {
                double t = 1.0 - ColorConverter.SmoothStep(0.12, 0.22, hsv.Value);
                p *= (1.0 - 0.55 * t);
            }

            return Math.Clamp(p, 0.0, 1.0);
        }

        // ---------- helpers ------------

        private static double SmoothGateLow (double x, double min, double softness)
        {
            return ColorConverter.SmoothStep(min - softness, min + softness, x);
        }

        private static double SmoothGateRange (double x, double min, double max, double softness)
        {
            double inScore = ColorConverter.SmoothStep(min - softness, min + softness, x);
            double outScore = 1.0 - ColorConverter.SmoothStep(max - softness, max + softness, x);

            return Math.Clamp(inScore * outScore, 0.0, 1.0);
        }

        private double HueSkinMembership (double hueDeg, double softnessDeg)
        {
            double a = SmoothHueRange(hueDeg, _model.HueLowA, _model.HueHighA, softnessDeg);
            double b = SmoothHueRange(hueDeg, _model.HueLowB, _model.HueHighB, softnessDeg);

            return Math.Max(a, b);
        }

        private static double SmoothHueRange (double h, double low, double high, double softnessDeg)
        {
            double inScore = ColorConverter.SmoothStep(low - softnessDeg, low + softnessDeg, h);
            double outScore = 1.0 - ColorConverter.SmoothStep(high - softnessDeg, high + softnessDeg, h);

            return Math.Clamp(inScore * outScore, 0.0, 1.0);
        }
    }
}