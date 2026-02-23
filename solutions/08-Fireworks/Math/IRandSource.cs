namespace _08_Fireworks.Math
{
    public interface IRandSource
    {
        double Next01 ();
        double Next (double min, double max);
        int NextInt (int minInc, int maxInc);
    }
}