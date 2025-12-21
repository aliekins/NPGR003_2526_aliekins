using System;
using System.IO;
using CommandLine;
using _06ImageRecoloring.Cli;
using _06ImageRecoloring.Pipeline;
using _06ImageRecoloring.Skin;
using _06ImageRecoloring.Transforms;

namespace _06ImageRecoloring
{
    internal static class Program
    {
        public static int Main (string[] args)
        {
            return Parser.Default.ParseArguments<Options>(args)
                .MapResult(Run, _ => 1);
        }

        private static int Run (Options opt)
        {
            if (opt is null)
            {
                Console.WriteLine("ERROR: Invalid args");
                return 1;
            }

            if (string.IsNullOrWhiteSpace(opt.Input))
            {
                Console.WriteLine("ERROR: Missing input file path");
                return 1;
            }

            if (string.IsNullOrWhiteSpace(opt.Output))
            {
                Console.WriteLine("ERROR: Missing output file path");
                return 1;
            }

            if (!File.Exists(opt.Input))
            {
                Console.WriteLine("ERROR: Input file not found: {opt.Input}");
                return 2;
            }

            SkinModel model = new SkinModel();
            ISkinDetector detector = new FuzzySkinDetector(model);
            IMaskedRecolorTransform transform = new HueShiftNonSkinTransform(opt.HueDeltaDegrees);

            ImageProcessor processor = new ImageProcessor(detector, transform);
            ProcessingResult result = processor.Process(opt.Input, opt.Output);

            if (result.Success)
            {
                Console.WriteLine(result.Message);
            }
            else
            {
                Console.Error.WriteLine(result.Message);
            }

            return result.ExitCode;
        }
    }
}