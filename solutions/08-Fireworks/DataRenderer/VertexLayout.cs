namespace _08_Fireworks.DataRenderer
{
    public static class VertexLayout
    {
        public const int FloatCountPerVertex = 12;

        // Offsets (in floats)
        public const int PosOffset = 0;      // 3
        public const int ColorOffset = 3;    // 3
        public const int NormalOffset = 6;   // 3
        public const int TxtOffset = 9;      // 2
        public const int SizeOffset = 11;    // 1
    }
}