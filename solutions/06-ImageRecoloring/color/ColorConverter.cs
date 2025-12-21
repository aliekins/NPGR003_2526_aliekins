using System;
using SixLabors.ImageSharp.PixelFormats;

namespace _06ImageRecoloring.Color
{
    internal static class ColorConverter
    {
        public static HsvColor ToHsv (Rgba32 px)
        {
            double r = px.R / 255.0;
            double g = px.G / 255.0;
            double b = px.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            double v = max;
            if (max <= 1e-12)
            {
                return new HsvColor(0.0, 0.0, 0.0);
            }

            double s = delta / max;
            if (delta <= 1e-12)
            {
                return new HsvColor(0.0, s, v);
            }

            double h;
            if (Math.Abs(max - r) < 1e-12)
            {
                h = 60.0 * ((g - b) / delta);
                if (h < 0.0)
                    h += 360.0;
            }
            else if (Math.Abs(max - g) < 1e-12)
            {
                h = 60.0 * (2.0 + (b - r) / delta);
            }
            else
            {
                h = 60.0 * (4.0 + (r - g) / delta);
            }

            return new HsvColor(WrapHue(h), s, v);
        }

        public static Rgba32 ToRgb (HsvColor hsv, byte alpha)
        {
            double h = WrapHue(hsv.Hue);
            double s = Clamp01(hsv.Saturation);
            double v = Clamp01(hsv.Value);

            if (s <= 1e-12)
            {
                byte gray = ToByte(v);
                return new Rgba32(gray, gray, gray, alpha);
            }

            double hh = h / 60.0;
            int sector = (int)Math.Floor(hh);
            double f = hh - sector;

            double p = v * (1.0 - s);
            double q = v * (1.0 - s * f);
            double t = v * (1.0 - s * (1.0 - f));

            (double r, double g, double b) = sector switch
            {
                0 => (v, t, p),
                1 => (q, v, p),
                2 => (p, v, t),
                3 => (p, q, v),
                4 => (t, p, v),
                _ => (v, p, q)
            };

            return new Rgba32(ToByte(r), ToByte(g), ToByte(b), alpha);
        }

        public static void ToYCbCr (Rgba32 px, out double y, out double cb, out double cr)
        {
            double r = px.R;
            double g = px.G;
            double b = px.B;

            y = 0.299 * r + 0.587 * g + 0.114 * b;
            cb = 128.0 - 0.168736 * r - 0.331264 * g + 0.5 * b;
            cr = 128.0 + 0.5 * r - 0.418688 * g - 0.081312 * b;
        }

        public static double NormalizeHueDelta (double degrees)
        {
            degrees %= 360.0;
            if (degrees <= -180.0)
            {
                degrees += 360.0;
            }
            if (degrees > 180.0)
            {
                degrees -= 360.0;
            }
            return degrees;
        }

        public static double WrapHue (double degrees)
        {
            degrees %= 360.0;
            if (degrees < 0.0)
            {
                degrees += 360.0;
            }
            return degrees;
        }

        public static double SmoothStep (double edge0, double edge1, double x)
        {
            if (Math.Abs(edge1 - edge0) < 1e-12)
            {
                return x < edge0 ? 0.0 : 1.0;
            }

            double t = (x - edge0) / (edge1 - edge0);
            t = Math.Clamp(t, 0.0, 1.0);
            return t * t * (3.0 - 2.0 * t);
        }

        private static double Clamp01 (double x) => Math.Clamp(x, 0.0, 1.0);

        private static byte ToByte (double x)
        {
            int v = (int)Math.Round(Math.Clamp(x, 0.0, 1.0) * 255.0);
            if (v < 0)
            {
                return 0;
            }
            if (v > 255)
            {
                return 255;
            }
            return (byte)v;
        }
    }
}
