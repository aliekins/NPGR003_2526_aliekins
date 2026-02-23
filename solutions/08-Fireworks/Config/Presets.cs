namespace _08_Fireworks.Config
{
    public static class Presets
    {
        public static FireworksConfig Default ()
        {
            var c = new FireworksConfig();

            c.Validate();

            return c;
        }

        public static FireworksConfig HighCrackle ()
        {
            var c = Default();
            c.Spark.CrackleRate = new RangeDouble(15.0, 60.0);
            c.Spark.StrobeHz = new RangeDouble(10.0, 30.0);

            c.Validate();

            return c;
        }

        public static FireworksConfig Willow ()
        {
            var c = Default();
            c.Spark.DragQuadratic = 0.6;
            c.Spark.Life = new RangeDouble(1.8, 3.5);

            c.Validate();

            return c;
        }
    }
}