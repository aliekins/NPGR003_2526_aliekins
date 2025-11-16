using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Xml;

namespace ImagePalette
{
    public static class PaletteTextWriter
    {
        public static void WriteToConsole(IReadOnlyList<Rgba32> colors)
        {
            foreach (var color in colors)
            {
                Console.WriteLine($"{color.R} {color.G} {color.B}");
            }
        }
    }

    public static class PaletteImageWriter
    {
        private const int ImageWidth = 600;
        private const int ImageHeight = 100;

        public static void SavePaletteImage(string fileName, IReadOnlyList<Rgba32> colors)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (extension == ".svg")
            {
                SaveAsSvg(fileName, colors);
            }
            else if (extension == ".png")
            {
                SaveAsPng(fileName, colors);
            }
            else
            {
                Console.Error.WriteLine("ERROR: Unsupported output format. Use .svg or .png.");
            }
        }

        private static void SaveAsSvg(string fileName, IReadOnlyList<Rgba32> colors)
        {
            int rectWidth = ImageWidth / Math.Max(1, colors.Count);
            int rectHeight = ImageHeight;

            var svgDoc = new XmlDocument();

            XmlElement svgRoot = svgDoc.CreateElement("svg");
            svgRoot.SetAttribute("xmlns", "http://www.w3.org/2000/svg");
            svgRoot.SetAttribute("width", ImageWidth.ToString());
            svgRoot.SetAttribute("height", ImageHeight.ToString());
            svgDoc.AppendChild(svgRoot);

            XmlElement group = svgDoc.CreateElement("g");
            svgRoot.AppendChild(group);

            for (int i = 0; i < colors.Count; i++)
            {
                Rgba32 color = colors[i];
                XmlElement rect = svgDoc.CreateElement("rect");
                rect.SetAttribute("x", (i * rectWidth).ToString());
                rect.SetAttribute("y", "0");
                rect.SetAttribute("width", rectWidth.ToString());
                rect.SetAttribute("height", rectHeight.ToString());
                rect.SetAttribute("fill", $"#{color.R:X2}{color.G:X2}{color.B:X2}");
                group.AppendChild(rect);
            }

            svgDoc.Save(fileName);
            Console.WriteLine($"SVG palette saved to '{fileName}'.");
        }

        private static void SaveAsPng(string fileName, IReadOnlyList<Rgba32> colors)
        {
            int rectWidth = ImageWidth / Math.Max(1, colors.Count);

            using Image<Rgba32> image = new Image<Rgba32>(ImageWidth, ImageHeight);

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgba32> row = accessor.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++)
                    {
                        int index = Math.Min(colors.Count - 1, x / rectWidth);
                        row[x] = colors[index];
                    }
                }
            });

            image.Save(fileName);
            Console.WriteLine($"PNG palette saved to '{fileName}'.");
        }
    }
}