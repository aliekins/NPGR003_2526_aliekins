using System;
using System.Globalization;
using CommandLine;
using _05Animation.Core;
using _05Animation.Cli;
using _05Animation.Styles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _05Animation
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

            int frames = options.Frames;
            if (frames < 1)
            {
                frames = 1;
            }

            for (int frameIndex = 0; frameIndex < frames; frameIndex++)
            {
                float time;
                if (frames == 1)
                {
                    time = 0f;
                }
                else
                {
                    time = frameIndex / (float)(frames - 1);
                }

                using var image = new Image<Rgba32>(config.Width, config.Height);

                style.Render(config, image, time);

                string path = string.Format(options.OutputMask, frameIndex);
                string? directory = Path.GetDirectoryName(path);

                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                image.Save(path);
                Console.WriteLine($"Saved {path}");
            }

            const string placeholder = "{0:0000}";
            string ffmpegMask = options.OutputMask;
            int idx = ffmpegMask.IndexOf(placeholder, StringComparison.Ordinal);

            if (idx >= 0)
            {
                ffmpegMask = ffmpegMask.Substring(0, idx) + "%04d" + ffmpegMask.Substring(idx + placeholder.Length);
            }

            Console.WriteLine();
            Console.WriteLine($"ffmpeg -framerate {options.Fps.ToString(CultureInfo.InvariantCulture)} -i \"{ffmpegMask}\" -c:v libx264 -pix_fmt yuv420p mandala.mp4");
        }
    }
}