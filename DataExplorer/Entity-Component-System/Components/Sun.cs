using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SpaceEngine.Core;
using SpaceEngine.Util;

namespace SpaceEngine.Entity_Component_System.Components
{
    internal class Sun : Component
    {   
        private Vector3 normalizedDirection = new Vector3(0f, 1f, 0f);

        private Vector3 sunColor = new Vector3(0.93f, 0.76f, 0.34f) * sunIntensity;
        private static float sunIntensity = 6f;
        private Vector3 sunScatterColor = new Vector3(1.0f, 0.9f, 0.7f)* sunIntensity;
        private Vector3 fogColor = new Vector3(0.5f, 0.6f, 0.7f) * sunIntensity;

        private Vector3 skyColorSunset = new Vector3(0.95f, 0.35f, 0f);
        //private Vector3 skyColorDay = new Vector3(0.27f, 0.71f, 0.96f);
        private Vector3 skyColorDay = new Vector3(0.1f, 0.2f, 0.3f);
        private Vector3 skyColorSpace = new Vector3(0.01f, 0.02f, 0.06f);
        private float ambient = 1.0f;
        private float fogDensity = 0.00045f;

        public float time = 0f;
        private float sunsetFactor = 1;

        public override void update(float delta)
        {
            ambient = 0.35f;
            sunsetFactor = (1f - MyMath.clamp01(MathF.Pow(1f - normalizedDirection.Y - 0.2f, 1)));
            sunsetFactor = 1f;
            if (InputHandler.isKeyDown(Keys.KeyPadAdd))
            {
                time += delta;
                Console.WriteLine("SunsetFactor: "+sunsetFactor);
            }

            if (InputHandler.isKeyDown(Keys.KeyPadSubtract))
            {
                time -= delta;
                Console.WriteLine("SunsetFactor: " + sunsetFactor);
            }

            float speed = Engine.EngineDeltaClock * 0.1f;
            speed = time;

            normalizedDirection.X = MathF.Sin(speed);
            normalizedDirection.Y = 0.3f;
            normalizedDirection.Z = MathF.Cos(speed);

            normalizedDirection.Normalize();
        }
        public float getSunsetFactor()
        {
            return sunsetFactor;
        }
        public float getFogDensity()
        {
            return fogDensity;
        }
        public float getAmbient()
        {
            return MyMath.clamp( ambient*sunsetFactor, 0.05f, ambient);
        }
        public Vector3 getSkyColorGround()
        {
            //return skyColorDay;
            return MyMath.lerp(getSunsetFactor(), skyColorSunset, skyColorDay);
        }
        public Vector3 getSkyColorSpace()
        {
            return skyColorSpace;
        }
        public Vector3 getSunColor()
        {
            return MyMath.lerp(getSunsetFactor(), skyColorSunset, sunColor)* sunsetFactor;
        }
        public Vector3 getSunScatterColor()
        {
            return MyMath.lerp(getSunsetFactor(), skyColorSunset, sunScatterColor)*sunsetFactor;
        }
        public Vector3 getFogColor()
        {
            return MyMath.lerp(getSunsetFactor(),skyColorSpace , skyColorDay);
            
        }
        public Vector3 getDirection()
        {
            return normalizedDirection;
        }
        public Vector3 getFullSunColor()
        {
            return MyMath.lerp(getSunsetFactor(), skyColorSunset, sunColor);
        }
    }
}
