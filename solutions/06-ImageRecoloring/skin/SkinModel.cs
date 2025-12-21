namespace _06ImageRecoloring.Skin
{
    public sealed class SkinModel
    {
        public double MeanCb { get; init; } = 108.0;
        public double MeanCr { get; init; } = 154.0;

        public double StdCb { get; init; } = 14.0;
        public double StdCr { get; init; } = 12.0;

        // Coarse gates
        public double MinY { get; init; } = 40.0;
        public double MinCb { get; init; } = 85.0;
        public double MaxCb { get; init; } = 135.0;
        public double MinCr { get; init; } = 135.0;
        public double MaxCr { get; init; } = 180.0;

        // HSV gates
        public double MinV { get; init; } = 0.04;
        public double MaxV { get; init; } = 0.85;
        public double MinS { get; init; } = 0.04;
        public double MaxS { get; init; } = 0.94;

        // Hue ranges considered skin
        public double HueLowA { get; init; } = 0.0;
        public double HueHighA { get; init; } = 55.0;
        public double HueLowB { get; init; } = 330.0;
        public double HueHighB { get; init; } = 360.0;
    }
}