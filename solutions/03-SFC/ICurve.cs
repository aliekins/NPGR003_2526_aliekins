namespace _03_SFC
{
    internal interface ICurve
    {
        string Name { get; }
        string Description { get; }
        bool IsSpaceFilling { get; }
        bool IsImplemented { get; }

        List<Vec2> Generate(int depth);
    }
}