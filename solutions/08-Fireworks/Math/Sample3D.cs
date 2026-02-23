using Silk.NET.Maths;

namespace _08_Fireworks.Math
{
    public static class Sample3D
    {
        // Uniform random direction on the unit sphere
        public static Vector3D<double> UnitVector (IRandSource rng)
        {
            // sample z in [-1,1], angle t in [0, 2pi)
            var z = rng.Next(-1.0, 1.0);
            var t = rng.Next(0.0, 2.0 * System.Math.PI);
            var r = System.Math.Sqrt(System.Math.Max(0.0, 1.0 - z * z));

            // then x,y from radius
            var x = r * System.Math.Cos(t);
            var y = r * System.Math.Sin(t);

            return new Vector3D<double>(x, y, z);
        }

        // Uniform random direction within a cone around axis with max angle angleRad
        public static Vector3D<double> InCone (IRandSource rng, Vector3D<double> axis, double angleRad)
        {
            // Normalize
            var a = axis;
            var len = a.Length;
            if (len < 1e-9)
            {
                a = new Vector3D<double>(0.0, 1.0, 0.0);
            }
            else
            {
                a /= len;
            }

            var cosMax = System.Math.Cos(angleRad);
            var u = rng.Next01();
            var cosTheta = cosMax + (1.0 - cosMax) * u;
            var sinTheta = System.Math.Sqrt(System.Math.Max(0.0, 1.0 - cosTheta * cosTheta));

            var phi = rng.Next(0.0, 2.0 * System.Math.PI);

            // Local direction around +Z then rotate basis so +Z 
            var local = new Vector3D<double>(
                sinTheta * System.Math.Cos(phi),
                sinTheta * System.Math.Sin(phi),
                cosTheta);

            return RotateFromZToAxis(local, a);
        }

        // Random point on a ring in a plane defined by normal, with optional thickness noise
        public static Vector3D<double> OnRing (IRandSource rng, Vector3D<double> normal, double radius, double thickness)
        {
            var n = normal;
            var nLen = n.Length;
            if (nLen < 1e-9)
            {
                n = new Vector3D<double>(0.0, 1.0, 0.0);
            }
            else
            {
                n /= nLen;
            }

            // Choose angle
            var t = rng.Next(0.0, 2.0 * System.Math.PI);

            // radius jitter in [-thickness, +thickness]
            var rr = radius + rng.Next(-thickness, thickness);

            // Build basis (u,v) spanning plane
            BuildOrthonormalBasis(n, out var u, out var v);

            return u * (rr * System.Math.Cos(t)) + v * (rr * System.Math.Sin(t));
        }

        // helpers

        private static void BuildOrthonormalBasis (Vector3D<double> n, out Vector3D<double> u, out Vector3D<double> v)
        {
            // Pick a vector not parallel to n
            var a = System.Math.Abs(n.Y) < 0.99 ? new Vector3D<double>(0.0, 1.0, 0.0) : new Vector3D<double>(1.0, 0.0, 0.0);

            u = Vector3D.Cross(n, a);
            var ul = u.Length;

            if (ul < 1e-9)
            {
                u = new Vector3D<double>(1.0, 0.0, 0.0);
            }
            else
            {
                u /= ul;
            }

            v = Vector3D.Cross(n, u);
        }

        private static Vector3D<double> RotateFromZToAxis (Vector3D<double> local, Vector3D<double> axis)
        {
            // map local basis where +Z is "forward" to axis direction
            BuildOrthonormalBasis(axis, out var u, out var v);
            var w = axis;

            // local x=u, local y=v, local z=w
            return u * local.X + v * local.Y + w * local.Z;
        }
    }
}