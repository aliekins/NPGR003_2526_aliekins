using CommandLine;
using _05Animation.Styles;

namespace _05Animation.Cli
{
    public class CLIOptions
    {
        [Option('w', "width", Required = true, HelpText = "Image width in pixels.")]
        public int Width { get; set; }

        [Option('h', "height", Required = true, HelpText = "Image height in pixels.")]
        public int Height { get; set; }

        [Option('p', "fps", Required = false, Default = 30.0, HelpText = "Frames per second (used when encoding video with ffmpeg).")]
        public double Fps { get; set; } = 30.0;

        [Option('f', "frames", Required = false, Default = 60, HelpText = "Total number of frames in the animation.")]
        public int Frames { get; set; } = 60;

        [Option('o', "output", Required = false, Default = "anim/out{0:0000}.png", HelpText = "Output filename mask for frames. Use C# format, e.g. anim/out{0:0000}.png")]
        public string OutputMask { get; set; } = "anim/out{0:0000}.png";

        [Option('s', "style", Required = false, Default = MandalaStyleKind.Celtic, HelpText = "Mandala style: Sand, Geometric, Hindu, Celtic, Lotus, Chakra, Tantric, Buddha")]
        public MandalaStyleKind Style { get; set; }

        [Option("seed", Required = false, HelpText = "Random seed for mandala generation.")]
        public int? Seed { get; set; }

        [Option("symmetry", Required = false, Default = 8, HelpText = "Rotational symmetry order (e.g. 5-12).")]
        public int Symmetry { get; set; } = 8;

        [Option("detail", Required = false, Default = 0.5, HelpText = "Level of detail [0,1].")]
        public double Detail { get; set; } = 0.5;
    }
}