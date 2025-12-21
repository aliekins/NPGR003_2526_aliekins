using CommandLine;

namespace _06ImageRecoloring.Cli
{
    public sealed class Options
    {
        [Option('i', "input", Required = true, HelpText = "Input image file.")]
        public string Input { get; set; } = string.Empty;

        [Option('o', "output", Required = true, HelpText = "Output image file.")]
        public string Output { get; set; } = string.Empty;

        [Option('h', "hue", Required = false, Default = 0.0, HelpText = "Hue delta in degrees (float). Positive/negative allowed. 0 = no recoloring; outputs skin mask for debugging.")]
        public double HueDeltaDegrees { get; set; } = 0.0;
    }
}