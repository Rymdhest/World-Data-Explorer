using SpaceEngine.Modelling;
using SpaceEngine.RenderEngine;
using OpenTK.Mathematics;
using SpaceEngine.Core;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SpaceEngine.Util;
using System.Drawing;
using SpaceEngine.Entity_Component_System.Systems;

namespace SpaceEngine.Entity_Component_System.Components
{
    internal class EntityManager
    {

        public static List<Entity> entities = new List<Entity>();
        public static ModelSystem flatShadingSystem = new ModelSystem();
        public static ComponentSystem smoothShadingSystem = new ComponentSystem();
        public static ComponentSystem postGeometrySystem = new ComponentSystem();
        public static ComponentSystem pointLightSystem = new ComponentSystem();
        public static Object threadLock = new object();
        public Entity sculpture = new Entity();
        public Entity sun;
        public Entity camera { get; set; }
        public EntityManager() {

            Vector3 center = new Vector3 (0, 0, 0);

            camera = new Entity();
            camera.addComponent(new Transformation(new Vector3(0f, 70f, 250f)+center, new Vector3(0.0f, 0, 0f)));;
            camera.addComponent(new InputMove());
            camera.addComponent(new Momentum());
            camera.addComponent(new HitBox(new Vector3(-2), new Vector3(2)));

            Entity plane = new Entity();
            plane.addComponent(new Transformation(new Vector3(0, -200, 0), new Vector3(-MathF.PI/2f, 0, 0), 1));
            plane.addComponent(new Model(glLoader.loadToVAO(MeshGenerator.generatePlane(new Vector2(400, 200))), MasterRenderer.Pipeline.SMOOTH_SHADING));

            Entity plane2 = new Entity();
            plane2.addComponent(new Transformation(new Vector3(0, -200, -400), new Vector3(-MathF.PI/4, 0, 0), 1));
            plane2.addComponent(new Model(glLoader.loadToVAO(MeshGenerator.generatePlane(new Vector2(400, 200))), MasterRenderer.Pipeline.SMOOTH_SHADING));


            Entity sun = new Entity();
            sun.addComponent(new Sun());
            this.sun = sun;


            for (int i = 0; i<= 0; i++)
            {
                for (int j = 0; j <= 0; j++)
                {
                    Entity Sphere = new Entity();
                    Mesh rawModel = MeshGenerator.generateEarth();
                    float spacing = 450f;
                    Sphere.addComponent(new Transformation(new Vector3(i* spacing, j* spacing, 0), new Vector3(0,0,0), 200f));
                    Sphere.addComponent(new Model(glLoader.loadToVAO(rawModel), MasterRenderer.Pipeline.SMOOTH_SHADING));
                }
            }
        }

        public void update(float delta)
        {
            if (InputHandler.isKeyDown(Keys.B))
            {
                for (int i = 0; i<300*delta; i++)
                {
                    Vector3 randOffset = MyMath.rng3DMinusPlus();
                    randOffset = camera.getComponent<Transformation>().createForwardVector(randOffset);
                    Vector3 forward = camera.getComponent<Transformation>().createForwardVector();

                    Vector3 center = camera.getComponent<Transformation>().position;
                    Vector3 color = new Vector3(MyMath.rng(), MyMath.rng(), MyMath.rng())*2;
                    Entity sphere = new Entity();
                    float power = (MyMath.rng() * 5f + 1) * 0.5f;
                    sphere.addComponent(new Transformation(center + forward*1.5f+randOffset*1f, new Vector3(0f, 0f, 0f), MathF.Sqrt(power)));
                    sphere.addComponent(new PointLight(color * power, new Vector3(0.1f, 0f, 1.5f)));
                    sphere.addComponent(new GlowEffect(color, MathF.Sqrt(power)));
                    sphere.addComponent(new Momentum(forward * 50f+randOffset*10f));

                    sphere.addComponent(new HitBox(new Vector3(-MathF.Sqrt(power) / 2f), new Vector3(MathF.Sqrt(power) / 2f)));
                }

            }

            postGeometrySystem.getMembers().Sort((v1, v2) => (v2.owner.getComponent<Transformation>().position - camera.getComponent<Transformation>().position).LengthSquared.CompareTo((v1.owner.getComponent<Transformation>().position - camera.getComponent<Transformation>().position).LengthSquared));


            //camera.updateComponents(delta);
            //lock (threadLock)
            {
                foreach (Entity entity in entities)
                {
                    entity.updateComponents(delta);
                }
            }

        }
    }
}
