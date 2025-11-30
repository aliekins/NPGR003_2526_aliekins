using System;
using CommandLine;
using _04Mandala.Core;
using _04Mandala.Cli;
using _04Mandala.Styles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _04Mandala
{
    internal class Program
    {
        private static int Main (string[] args)
        {
            int exitCode = 0;

            Parser.Default.ParseArguments<CLIOptions>(args)
                .WithParsed(opts =>
                {
                    try
                    {
                        Run(opts);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error: {ex.Message}");
                        exitCode = 1;
                    }
                })
                .WithNotParsed(_ =>
                {
                    exitCode = 1;
                });

            return exitCode;
        }

        private static void Run (CLIOptions options)
        {
            int defaultSeed = Environment.TickCount;
            var config = MandalaConfig.FromCLIOptions(options, defaultSeed);

            IMandalaStyle style = StyleFactory.Create(config.Style);

            using var image = new Image<Rgba32>(config.Width, config.Height);
            style.Render(config, image);

            image.Save(options.OutputPath);
            Console.WriteLine($"Saved {options.OutputPath}");
        }
    }
}