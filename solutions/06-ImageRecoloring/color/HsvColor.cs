namespace _06ImageRecoloring.Color
{
    public readonly struct HsvColor
    {
        public HsvColor (double hue, double saturation, double value)
        {
            Hue = hue;
            Saturation = saturation;
            Value = value;
        }

        public double Hue { get; }
        public double Saturation { get; }
        public double Value { get; }

        public HsvColor WithHue (double hue) => new HsvColor(hue, Saturation, Value);
        public HsvColor WithSaturation (double saturation) => new HsvColor(Hue, saturation, Value);
        public HsvColor WithValue (double value) => new HsvColor(Hue, Saturation, value);
    }
}