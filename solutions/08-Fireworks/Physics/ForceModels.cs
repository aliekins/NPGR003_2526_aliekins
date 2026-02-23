using Silk.NET.Maths;

namespace _08_Fireworks.Physics
{
    public static class ForceModels
    {
        public static Vector3D<double> Gravity (double mass, Vector3D<double> gravity)
        {
            return gravity * mass;
        }

        // Drag force: Fd = -k1 * v - k2 * |v| * v
        // (linear = laminar, quadratic = turbulent)
        public static Vector3D<double> Drag (Vector3D<double> velocity, double k1, double k2)
        {
            var v = velocity;

            // Linear
            var f = -k1 * v;

            // Quadratic
            var speed = v.Length;
            if (speed > 1e-9)
            {
                f += -k2 * speed * v;
            }
            return f;
        }
    }
}