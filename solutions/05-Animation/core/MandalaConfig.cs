using _05Animation.Cli;
using _05Animation.Styles;

namespace _05Animation.Core
{
    public class MandalaConfig
    {
        public int Width { get; }
        public int Height { get; }
        public MandalaStyleKind Style { get; }
        public int? Seed { get; }
        public int Symmetry { get; }
        public double Detail { get; }

        public MandalaConfig (int width, int height, MandalaStyleKind styleKind, int? seed, int symmetry, double detail)
        {
            Width = width;
            Height = height;
            Style = styleKind;
            Seed = seed;
            Symmetry = symmetry;
            Detail = detail;
        }

        public static MandalaConfig FromCLIOptions (CLIOptions options, int defaultSeed)
        {
            var seed = options.Seed ?? defaultSeed;

            return new MandalaConfig(
                options.Width,
                options.Height,
                options.Style,
                seed,
                options.Symmetry,
                options.Detail
            );
        }
    }
}