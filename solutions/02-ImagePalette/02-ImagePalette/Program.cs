using System;
using System.Xml;
using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImagePalette
{

    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Input image file name.")]
        public string InputFileName { get; set; } = string.Empty;

        [Option('o', "output", Required = false, Default = "", HelpText = "Output file-name (PNG or SVG).")]
        public string OutputFileName { get; set; } = string.Empty;

        [Option('c', "colors", Required = false, Default = 5, HelpText = "Required number of colors (3–10 is recommended).")]
        public int ColorCount { get; set; }

        [Option('r', "recreate", Required = false, Default = "", HelpText = "Recreated image output file-name (quantized using palette).")]
        public string RecreateFileName { get; set; } = string.Empty;
    }

    class Program
    {
        public static int Main(string[] args)
        {
            return Parser.Default
              .ParseArguments<Options>(args)
              .MapResult(
                Run,
                _ => 1
              );
        }

        private static int Run(Options options)
        {
            if (!File.Exists(options.InputFileName))
            {
                Console.WriteLine($"ERROR: Input file '{options.InputFileName}' does not exist.");
                return 1;
            }

            if (options.ColorCount <= 0)
            {
                Console.WriteLine("ERROR: Color count must be a positive integer.");
                return 1;
            }

            using Image<Rgba32> image = Image.Load<Rgba32>(options.InputFileName);

            IReadOnlyList<Rgba32> palette = PaletteGenerator.GeneratePalette(image, options.ColorCount);
            PaletteTextWriter.WriteToConsole(palette);

            if (!string.IsNullOrEmpty(options.OutputFileName))
            {
                PaletteImageWriter.SavePaletteImage(options.OutputFileName, palette);
            }

            if (!string.IsNullOrEmpty(options.RecreateFileName))
            {
                ImageRecreator.SaveRecreatedImage(options.RecreateFileName, image, palette);
                Console.WriteLine($"Recreated image saved to '{options.RecreateFileName}'.");
            }

            return 0;
        }
    }
}