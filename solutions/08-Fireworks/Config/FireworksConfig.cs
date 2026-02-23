using Silk.NET.Maths;

namespace _08_Fireworks.Config
{
    public sealed class FireworksConfig
    {
        // constraints

        public int MaxParticles = 100000;
        public double MaxDt = 1.0 / 30.0;

        // If pool is almost full, reduce spawning automatically
        public double SpawnThrottleStartRatio = 0.90; // start throttling at 90%
        public double SpawnThrottleHardRatio = 0.98;  // almost no spawning at 98%

        // Global physics
        public Vector3D<double> Gravity = new Vector3D<double>(0.0, -9.81, 0.0);

        // particle type configs

        public RocketConfig Rocket = new RocketConfig();
        public BombletConfig Bomblet = new BombletConfig();
        public SparkConfig Spark = new SparkConfig();
        public MicroSparkConfig MicroSpark = new MicroSparkConfig();
        public RenderConfig Render = new RenderConfig();

        public void Validate ()
        {
            if (MaxParticles <= 0)
            {
                throw new System.ArgumentException("MaxParticles must be > 0.");
            }
            if (MaxDt <= 0)
            {
                throw new System.ArgumentException("MaxDt must be > 0.");
            }

            Rocket.Validate();
            Bomblet.Validate();
            Spark.Validate();
            MicroSpark.Validate();
            Render.Validate();
        }
    }

    // Per particle-type configs
    public sealed class RocketConfig
    {
        // physics
        public double Mass = 1.0;
        public double DragLinear = 0.05;
        public double DragQuadratic = 0.02;

        // life and fuse
        public RangeDouble Life = new RangeDouble(2.0, 3.5);
        public RangeDouble Fuse = new RangeDouble(0.2, 1.5);

        // init velocity
        public RangeDouble Speed = new RangeDouble(5.0, 10.0);

        // First-stage burst
        public RangeInt BombletCount = new RangeInt(8, 20);
        public RangeDouble BombletBurstSpeed = new RangeDouble(2.0, 4.0);

        // Velocity inheritance: bombletVel += rocketVel * Inherit
        public double InheritVelocity = 0.30;

        public void Validate ()
        {
            if (Mass <= 0)
            {
                throw new System.ArgumentException("Rocket.Mass must be > 0.");
            }
            if (Life.Min <= 0 || Life.Max <= 0 || Life.Min > Life.Max)
            {
                throw new System.ArgumentException("Rocket.Life invalid.");
            }
            if (Fuse.Min <= 0 || Fuse.Max <= 0 || Fuse.Min > Fuse.Max)
            {
                throw new System.ArgumentException("Rocket.Fuse invalid.");
            }
        }
    }

    public sealed class BombletConfig
    {
        public double Mass = 0.6;
        public double DragLinear = 0.08;
        public double DragQuadratic = 0.06;

        public RangeDouble Life = new RangeDouble(0.8, 2.0);
        public RangeDouble Fuse = new RangeDouble(0.25, 1.2);

        // Second-stage burst
        public RangeInt SparkCount = new RangeInt(40, 140);
        public RangeDouble SparkBurstSpeed = new RangeDouble(6.0, 15.0);

        public double InheritVelocity = 0.20;

        public void Validate ()
        {
            if (Mass <= 0)
            {
                throw new System.ArgumentException("Bomblet.Mass must be > 0.");
            }
            if (Life.Min <= 0 || Life.Max <= 0 || Life.Min > Life.Max)
            {
                throw new System.ArgumentException("Bomblet.Life invalid.");
            }
            if (Fuse.Min <= 0 || Fuse.Max <= 0 || Fuse.Min > Fuse.Max)
            {
                throw new System.ArgumentException("Bomblet.Fuse invalid.");
            }
        }
    }

    public sealed class SparkConfig
    {
        public double Mass = 0.2;
        public double DragLinear = 0.15;
        public double DragQuadratic = 0.30;

        public RangeDouble Life = new RangeDouble(0.8, 2.4);

        // Appearance base
        public RangeDouble Size = new RangeDouble(2.0, 6.0);

        // Strobe (render only)
        public RangeDouble StrobeHz = new RangeDouble(0.0, 25.0);
        public double StrobeDuty = 0.35;  // fraction of cycle "on"
        public bool StrobeHard = true;    // hard flicker vs smooth

        // Crackle (spawning)
        public RangeDouble CrackleRate = new RangeDouble(0.0, 30.0); // micro sparks per second
        public RangeInt CrackleBurstCount = new RangeInt(1, 3); // micro sparks per event

        public void Validate ()
        {
            if (Mass <= 0)
            {
                throw new System.ArgumentException("Spark.Mass must be > 0.");
            }
            if (Life.Min <= 0 || Life.Max <= 0 || Life.Min > Life.Max)
            {
                throw new System.ArgumentException("Spark.Life invalid.");
            }
            if (StrobeDuty < 0 || StrobeDuty > 1)
            {
                throw new System.ArgumentException("Spark.StrobeDuty must be in [0,1].");
            }
        }
    }

    public sealed class MicroSparkConfig
    {
        public double Mass = 0.05;
        public double DragLinear = 0.20;
        public double DragQuadratic = 0.40;

        public RangeDouble Life = new RangeDouble(0.08, 0.40);
        public RangeDouble Speed = new RangeDouble(1.0, 7.0);
        public RangeDouble Size = new RangeDouble(1.0, 2.5);

        public void Validate ()
        {
            if (Mass <= 0)
            {
                throw new System.ArgumentException("MicroSpark.Mass must be > 0.");
            }
            if (Life.Min <= 0 || Life.Max <= 0 || Life.Min > Life.Max)
            {
                throw new System.ArgumentException("MicroSpark.Life invalid.");
            }
        }
    }

    // Render configuration
    public sealed class RenderConfig
    {
        // Age fade
        public bool FadeToBlack = true;
        public bool ShrinkOverLife = true;

        public float MinPointSize = 1.0f;
        public float MaxPointSize = 12.0f;

        public void Validate ()
        {
            if (MinPointSize <= 0)
            {
                throw new System.ArgumentException("Render.MinPointSize must be > 0.");
            }
            if (MaxPointSize < MinPointSize)
            {
                throw new System.ArgumentException("Render.MaxPointSize invalid.");
            }
        }
    }
}