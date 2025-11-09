using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;

using AllTheColors.Validator;
using AllTheColors.FillStrategies;


namespace AllTheColors
{
    /* --------------- OPTIONS - CLI --------------- */
    public class Options
    {
        [Option('w', "width", Required = false, Default = 4096, HelpText = "Image width in pixels.")]
        public int Width { get; set; }

        [Option('h', "height", Required = false, Default = 4096, HelpText = "Image height in pixels.")]
        public int Height { get; set; }

        [Option('o', "output", Required = false, Default = "xxx.png", HelpText = "Output file name.")]
        public string FileName { get; set; } = "xxx.png";

        [Option('m', "mode", Required = false, Default = "trivial", HelpText = "Mode: trivial | random | pattern")]
        public string Mode { get; set; } = "trivial";

        // random
        [Option("seed", Required = false, HelpText = "Random seed (only for random mode).")]
        public int? Seed { get; set; }

        // pattern
        [Option("pattern-style", Required = false, Default = "snake", HelpText = "pattern style: snake | diagonal | spiral | blocks")]
        public string PatternStyle { get; set; } = "snake";

        [Option("pattern-block-size", Required = false, Default = 64, HelpText = "block size for pattern=blocks")]
        public int PatternBlockSize { get; set; }

        // mandala
        [Option("mandala-arms", Required = false, Default = 8, HelpText = "Number of symmetry arms for mandala mode.")]
        public int MandalaArms { get; set; }

        [Option("mandala-center-x", Required = false, Default = -1, HelpText = "Center X for mandala (default = middle).")]
        public int MandalaCenterX { get; set; }

        [Option("mandala-center-y", Required = false, Default = -1, HelpText = "Center Y for mandala (default = middle).")]
        public int MandalaCenterY { get; set; }


        // ornament
        [Option("ornament-depth", Required = false, Default = 3, HelpText = "Recursion depth for ornament mode.")]
        public int OrnamentDepth { get; set; }

        [Option("ornament-min", Required = false, Default = 32, HelpText = "Minimal region size for ornament mode.")]
        public int OrnamentMinSize { get; set; }
    }

    /* --------------- PROGRAM --------------- */
    internal class Program
    {
        private const int AllColorsCount = 256 * 256 * 256;

        static int Main (string[] args)
        {
            return Parser
              .Default
              .ParseArguments<Options>(args)
              .MapResult(
                o =>
                    {
                        Run(o);
                        return 0;
                    },
                    errs =>
                    {
                        Console.WriteLine("ERROR failed to parse arguments.");
                        return 0;
                    });
        }

        private static void Run (Options o)
        {
            var validationResult = ImageRequestValidator.ValidateDimensions(o.Width, o.Height);
            if (!validationResult.IsValid)
            {
                Console.WriteLine("ERROR " + validationResult.ErrorMessage);
                return;
            }

            var strategy = FillStrategyFactory.Create(o);
            if (strategy == null)
            {
                Console.WriteLine("ERROR unknown mode: " + o.Mode);
                return;
            }

            try
            {
                using (var image = new Image<Rgba32>(o.Width, o.Height))
                {
                    strategy.Fill(image);
                    image.Save(o.FileName);
                }

                Console.WriteLine("Image '" + o.FileName + "' created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR " + ex.Message);
            }
        }
    }
}