using CommandLine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace _03_SFC
{
    public class Options
    {
        [Option('o', "output", Required = false, Default = "output.svg",
          HelpText = "Output file-name (SVG).")]
        public string FileName { get; set; } = "output.svg";

        [Option('w', "width", Required = false, Default = 400,
          HelpText = "Image width.")]
        public int Width { get; set; } = 400;

        [Option('h', "height", Required = false, Default = 400,
          HelpText = "Image height.")]
        public int Height { get; set; } = 400;

        [Option('c', "curve", Required = false, Default = "hilbert",
          HelpText = "Curve type (name or numeric index).")]
        public string Curve { get; set; } = "hilbert";

        [Option('d', "depth", Required = false, Default = 4,
          HelpText = "Recursion depth / detail level.")]
        public int Depth { get; set; } = 4;

        [Option("color", Required = false, Default = "#00FFAA",
          HelpText = "Base curve color in #RRGGBB.")]
        public string Color { get; set; } = "#00FFAA";

        [Option("background", Required = false, Default = "#000000",
          HelpText = "Background color in #RRGGBB.")]
        public string Background { get; set; } = "#000000";

        [Option("thickness", Required = false, Default = 1.5,
          HelpText = "Base line thickness in pixels.")]
        public double Thickness { get; set; } = 1.5;
    }

    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
              .WithParsed(RunSafely);
            // On parse errors, CommandLine prints help and WithParsed is not called
        }

        private static void RunSafely(Options o)
        {
            try
            {
                Run(o);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Unexpected ERROR: " + ex.Message);
            }
        }

        private static void Run(Options o)
        {
            if (!o.FileName.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            {
                Console.Error.WriteLine("ERROR: Output file name must end with .svg");
                return;
            }

            if (o.Width <= 0 || o.Height <= 0)
            {
                Console.Error.WriteLine("ERROR: Width and height must be positive integers.");
                return;
            }

            if (o.Depth < 0)
            {
                Console.Error.WriteLine("ERROR: Depth must be non-negative.");
                return;
            }

            ICurve curve;
            try
            {
                curve = CurveRegistry.Resolve(o.Curve);
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine("ERROR: " + ex.Message);
                Console.Error.WriteLine("Available curves:");
                CurveRegistry.PrintAvailable();
                return;
            }

            if (!curve.IsImplemented)
            {
                Console.Error.WriteLine(
                  $"Curve '{curve.Name}' is registered but not implemented yet: {curve.Description}");
                Console.Error.WriteLine("Choose another curve for now.");
                return;
            }

            List<Vec2> points;
            try
            {
                points = curve.Generate(o.Depth);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error while generating the curve: " + ex.Message);
                return;
            }

            if (points == null || points.Count < 2)
            {
                Console.Error.WriteLine("ERROR: Generated too few points to draw a curve.");
                return;
            }

            XmlDocument svgDoc = new XmlDocument();

            XmlElement svgRoot = svgDoc.CreateElement("svg");
            svgRoot.SetAttribute("xmlns", "http://www.w3.org/2000/svg");
            svgRoot.SetAttribute("width", o.Width.ToString(CultureInfo.InvariantCulture));
            svgRoot.SetAttribute("height", o.Height.ToString(CultureInfo.InvariantCulture));
            svgDoc.AppendChild(svgRoot);

            // Background
            XmlElement bg = svgDoc.CreateElement("rect");
            bg.SetAttribute("x", "0");
            bg.SetAttribute("y", "0");
            bg.SetAttribute("width", o.Width.ToString(CultureInfo.InvariantCulture));
            bg.SetAttribute("height", o.Height.ToString(CultureInfo.InvariantCulture));
            bg.SetAttribute("fill", o.Background);
            svgRoot.AppendChild(bg);

            int squareSize = Math.Max(Math.Min(o.Width, o.Height) - 10, 10);

            XmlElement group = svgDoc.CreateElement("g");
            int offsetX = (o.Width - squareSize) / 2;
            int offsetY = (o.Height - squareSize) / 2;
            group.SetAttribute("transform", $"translate({offsetX},{offsetY})");
            svgRoot.AppendChild(group);

            if (curve is BarnsleyFernCurve)
            {
                SvgRenderer.DrawPoints(svgDoc, group, points, squareSize, o.Color, o.Thickness);
            }
            else
            {
                SvgRenderer.DrawCurve(svgDoc, group, points, squareSize, o.Color, o.Thickness);
            }

            try
            {
                svgDoc.Save(o.FileName);
                Console.WriteLine(
                  $"SVG saved to {o.FileName} (curve={curve.Name}, depth={o.Depth}).");
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine("Error writing SVG file: " + ex.Message);
            }
        }
    }
}