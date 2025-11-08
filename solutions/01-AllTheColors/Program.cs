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

        [Option('o', "output", Required = false, Default = "allrgb.png", HelpText = "Output file name.")]
        public string FileName { get; set; } = "allrgb.png";

        [Option('m', "mode", Required = false, Default = "trivial", HelpText = "Mode: trivial | random | pattern")]
        public string Mode { get; set; } = "trivial";

        [Option("seed", Required = false, HelpText = "Random seed (only for random mode).")]
        public int? Seed { get; set; }
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

            string mode = o.Mode;
            if (mode == null)
            {
                mode = "trivial";
            }
            mode = mode.ToLowerInvariant();

            var strategy = FillStrategyFactory.Create(mode, o.Seed);
            if (strategy == null)
            {
                Console.WriteLine("ERROR unknown mode: " + mode);
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