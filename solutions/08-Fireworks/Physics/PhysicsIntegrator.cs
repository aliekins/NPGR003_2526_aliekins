using Silk.NET.Maths;

namespace _08_Fireworks.Physics
{
    public static class PhysicsIntegrator
    {
        // Steps one particle's position/velocity by dt seconds
        public static void Step (ref Vector3D<double> position, ref Vector3D<double> velocity, double mass, Vector3D<double> gravity, double dragLinear, double dragQuadratic, double dt)
        {
            if (dt <= 0.0)
            {
                return;
            }
            if (mass <= 0.0)
            {
                return; // config should prevent this
            }

            // Forces
            var fg = ForceModels.Gravity(mass, gravity);
            var fd = ForceModels.Drag(velocity, dragLinear, dragQuadratic);

            // Acceleration
            var a = (fg + fd) / mass;

            // Integrate (uniform accel for position + Euler for velocity)
            position += velocity * dt + 0.5 * a * dt * dt;
            velocity += a * dt;
        }
    }
}