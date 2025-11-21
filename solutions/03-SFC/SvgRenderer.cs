using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace _03_SFC
{
    internal static class SvgRenderer
    {
        public static void DrawCurve(XmlDocument doc, XmlElement parentGroup, List<Vec2> points, int squareSize, string baseColorHex, double baseThickness)
        {
            if (points == null || points.Count < 2)
                throw new ArgumentException("Need at least two points to draw a curve.", nameof(points));

            double minX = double.PositiveInfinity;
            double minY = double.PositiveInfinity;
            double maxX = double.NegativeInfinity;
            double maxY = double.NegativeInfinity;

            foreach (var p in points)
            {
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
            }

            if (double.IsInfinity(minX) || double.IsInfinity(minY))
                throw new InvalidOperationException("Invalid point set for SVG rendering.");

            double dx = maxX - minX;
            double dy = maxY - minY;
            if (dx <= 0.0) dx = 1.0;
            if (dy <= 0.0) dy = 1.0;

            double margin = squareSize * 0.05; // 5% margin
            double scale = Math.Min(
              (squareSize - 2 * margin) / dx,
              (squareSize - 2 * margin) / dy);

            // parse base color
            (int baseR, int baseG, int baseB) = ParseHexColor(baseColorHex);

            int segments = points.Count - 1;
            if (segments <= 0) return;

            for (int i = 0; i < segments; i++)
            {
                Vec2 p1 = points[i];
                Vec2 p2 = points[i + 1];

                double x1 = (p1.X - minX) * scale + margin;
                double y1 = (p1.Y - minY) * scale + margin;
                double x2 = (p2.X - minX) * scale + margin;
                double y2 = (p2.Y - minY) * scale + margin;

                double t = segments > 1 ? (double)i / (segments - 1) : 0.0;
                t = Clamp01(t);

                double brightness = 0.4 + 0.6 * t;
                int r = ClampByte((int)(baseR * brightness));
                int g = ClampByte((int)(baseG * brightness));
                int b = ClampByte((int)(baseB * brightness));

                double thickFactor = 0.7 + 0.8 * Math.Sin(Math.PI * t) * Math.Sin(Math.PI * t);
                double strokeWidth = baseThickness * thickFactor;

                XmlElement line = doc.CreateElement("line");
                line.SetAttribute("x1", x1.ToString("F3", CultureInfo.InvariantCulture));
                line.SetAttribute("y1", y1.ToString("F3", CultureInfo.InvariantCulture));
                line.SetAttribute("x2", x2.ToString("F3", CultureInfo.InvariantCulture));
                line.SetAttribute("y2", y2.ToString("F3", CultureInfo.InvariantCulture));
                line.SetAttribute("stroke",
                  $"rgb({r},{g},{b})");
                line.SetAttribute("stroke-width",
                  strokeWidth.ToString("F2", CultureInfo.InvariantCulture));
                line.SetAttribute("stroke-linecap", "round");
                parentGroup.AppendChild(line);
            }
        }

        public static void DrawPoints(XmlDocument doc, XmlElement parentGroup, List<Vec2> points, int squareSize, string baseColorHex, double baseThickness)
        {
            if (points == null || points.Count < 1)
                throw new ArgumentException("Need at least one point to draw.", nameof(points));

            double minX = double.PositiveInfinity;
            double minY = double.PositiveInfinity;
            double maxX = double.NegativeInfinity;
            double maxY = double.NegativeInfinity;

            foreach (var p in points)
            {
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
            }

            if (double.IsInfinity(minX) || double.IsInfinity(minY))
                throw new InvalidOperationException("Invalid point set for SVG rendering.");

            double dx = maxX - minX;
            double dy = maxY - minY;
            if (dx <= 0.0) dx = 1.0;
            if (dy <= 0.0) dy = 1.0;

            double margin = squareSize * 0.05;
            double scale = Math.Min(
                (squareSize - 2 * margin) / dx,
                (squareSize - 2 * margin) / dy);

            // base color
            (int baseR, int baseG, int baseB) = ParseHexColor(baseColorHex);

            double radius = baseThickness * (squareSize / 400.0);
            if (radius < 0.3) radius = 0.3;

            int count = points.Count;
            for (int i = 0; i < count; i++)
            {
                Vec2 p = points[i];

                double x = (p.X - minX) * scale + margin;
                double y = (p.Y - minY) * scale + margin;

                double t = (count > 1) ? (double)i / (count - 1) : 0.0;
                t = Clamp01(t);

                double brightness = 0.5 + 0.5 * t;
                int r = ClampByte((int)(baseR * brightness));
                int g = ClampByte((int)(baseG * brightness));
                int b = ClampByte((int)(baseB * brightness));

                XmlElement circle = doc.CreateElement("circle");
                circle.SetAttribute("cx", x.ToString("F3", CultureInfo.InvariantCulture));
                circle.SetAttribute("cy", y.ToString("F3", CultureInfo.InvariantCulture));
                circle.SetAttribute("r", radius.ToString("F3", CultureInfo.InvariantCulture));
                circle.SetAttribute("fill", $"rgb({r},{g},{b})");
                circle.SetAttribute("stroke", "none");
                parentGroup.AppendChild(circle);
            }
        }

        private static (int r, int g, int b) ParseHexColor(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return (0, 255, 170);

            string s = hex.Trim();
            if (s.StartsWith("#")) s = s.Substring(1);
            if (s.Length != 6)
                return (0, 255, 170);

            try
            {
                int r = int.Parse(s.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                int g = int.Parse(s.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                int b = int.Parse(s.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                return (ClampByte(r), ClampByte(g), ClampByte(b));
            }
            catch
            {
                return (0, 255, 170);
            }
        }

        private static int ClampByte(int v)
        {
            if (v < 0) return 0;
            if (v > 255) return 255;
            return v;
        }

        private static double Clamp01(double t)
        {
            if (t < 0.0) return 0.0;
            if (t > 1.0) return 1.0;
            return t;
        }
    }
}