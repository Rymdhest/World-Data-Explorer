using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using SpaceEngine.Modelling;
using SpaceEngine.RenderEngine;
using SpaceEngine.Entity_Component_System.Components;
using SpaceEngine.Util;
using DataExplorer.DataEarth;

namespace SpaceEngine.Core
{
    internal class Engine
    {
        private MasterRenderer masterRenderer;
        private WindowHandler windowHandler;
        private InputHandler inputHandler;
        private EntityManager entityManager;
        public static Earth earth;
        public static float EngineDeltaClock = 0f;

        public Engine()
        {
            Vector2i resoltion = new Vector2i(1900, 1080);
            windowHandler = new WindowHandler(resoltion);
            masterRenderer = new MasterRenderer();
            entityManager= new EntityManager();
            inputHandler = new InputHandler();

            earth = new Earth();

            WindowHandler.getWindow().Load += delegate
            {
                init();
            };
            WindowHandler.getWindow().UpdateFrame += delegate (FrameEventArgs eventArgs)
            {
                update((float)eventArgs.Time);
            };
            WindowHandler.getWindow().RenderFrame += delegate (FrameEventArgs eventArgs)
            {
                render();
            };
            WindowHandler.getWindow().KeyDown += delegate (KeyboardKeyEventArgs eventArgs)
            {
                inputHandler.keyDown(eventArgs);
            };
            WindowHandler.getWindow().KeyUp += delegate (KeyboardKeyEventArgs eventArgs)
            {
                inputHandler.keyUp(eventArgs);
            };
            WindowHandler.getWindow().Resize += delegate (ResizeEventArgs eventArgs)
            {
                windowHandler.onResize(eventArgs);
                masterRenderer.onResize(eventArgs);
            };
        }

        public void run()
        {
            WindowHandler.getWindow().Run();
        }

        private void init()
        {

        }

        private void update(float delta)
        {
            EngineDeltaClock += delta;
            windowHandler.update(delta);
            entityManager.update(delta);
            masterRenderer.update(delta);
            inputHandler.update(delta);
        }

        private void render()
        {
            windowHandler.render();
            Matrix4 viewMatrix = MyMath.createViewMatrix(entityManager.camera.getComponent<Transformation>());
            masterRenderer.render(viewMatrix, entityManager.camera, entityManager.sun, EntityManager.pointLightSystem); 
        }
    }
}
