using Silk.NET.Maths;

namespace _08_Fireworks.Launchers
{
    public readonly struct LauncherInput
    {
        public readonly bool FireHeld;
        public readonly bool FirePressed;
        public readonly Vector3D<double> AimDirection;

        public LauncherInput (bool fireHeld, bool firePressed, Vector3D<double> aimDirection)
        {
            FireHeld = fireHeld;
            FirePressed = firePressed;
            AimDirection = aimDirection;
        }
    }
}
