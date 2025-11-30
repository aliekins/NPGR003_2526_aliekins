using CommandLine;
using _04Mandala.Styles;


namespace _04Mandala.Cli
{
    public class CLIOptions
    {
        [Option('w', "width", Required = true, HelpText = "Image width in pixels.")]
        public int Width { get; set; }

        [Option('h', "height", Required = true, HelpText = "Image height in pixels.")]
        public int Height { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file path for the generated mandala image.")]
        public string OutputPath { get; set; } = string.Empty;


        [Option('s', "style", Required = false, Default = MandalaStyleKind.Geometric, HelpText = "Mandala style: Sand, Geometric, Hindu, Celtic, Lotus, Chakra, Tantric, Buddha.")]
        public MandalaStyleKind Style { get; set; }

        [Option("seed", Required = false, HelpText = "Random seed for mandala generation.")]
        public int? Seed { get; set; }

        [Option("symmetry", Required = false, Default = 8, HelpText = "Rotational symmetry order (e.g. 5–12).")]
        public int Symmetry { get; set; }

        [Option("detail", Required = false, Default = 0.5, HelpText = "Level of detail [0,1].")]
        public double Detail { get; set; }
    }
}