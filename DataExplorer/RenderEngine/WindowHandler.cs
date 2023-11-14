
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using SpaceEngine.Entity_Component_System.Components;
using System.Diagnostics;
using OpenTK.Input;
namespace SpaceEngine.RenderEngine
{
    internal class WindowHandler
    {
        private string title = "SpaceEngine";
        public static GameWindow? gameWindow = null;

        private Stopwatch frameStopWatch = new Stopwatch();
        private Stopwatch secondStopWatchUpdate = new Stopwatch();
        private Stopwatch secondStopWatchRender = new Stopwatch();
        private float delta = 0f;
        private int framesLastSecondUpdate = 0;
        private int framesLastSecondRender = 0;
        private int framesCurrentSecondUpate = 0;
        private int framesCurrentSecondRender = 0;
        public static Vector2i resolution;

        public WindowHandler(Vector2i resolution)
        {
            GameWindowSettings gws = GameWindowSettings.Default;
            NativeWindowSettings nws = NativeWindowSettings.Default;
            WindowHandler.resolution = resolution;
            nws.API = ContextAPI.OpenGL;
            //nws.APIVersion = Version.Parse("3.3");
            nws.AutoLoadBindings = true;
            nws.Title = title;
            nws.Size = resolution;
            nws.Location = new Vector2i(100, 100);

            gws.UpdateFrequency = 60;

            gameWindow = new GameWindow(gws, nws);

            secondStopWatchUpdate.Start();
            secondStopWatchRender.Start();
            frameStopWatch.Start();
        }
        public static GameWindow getWindow()
        {
            return gameWindow;
        }
        public void update(float delta)
        {
            this.delta = (float)frameStopWatch.Elapsed.TotalSeconds;
            frameStopWatch.Restart();

            if (secondStopWatchUpdate.Elapsed.TotalMilliseconds >= 1000.0)
            {
                framesLastSecondUpdate = framesCurrentSecondUpate;
                framesCurrentSecondUpate = 0;
                gameWindow.Title = title + " " + framesLastSecondUpdate + " FPS Update : "+ framesLastSecondRender+" FPS Render : "+EntityManager.entities.Count+" Entities";
                secondStopWatchUpdate.Restart();

            }
            framesCurrentSecondUpate++;
        }
        public void render()
        {
            if (secondStopWatchRender.Elapsed.TotalMilliseconds >= 1000.0)
            {
                framesLastSecondRender = framesCurrentSecondRender;
                framesCurrentSecondRender = 0;
                secondStopWatchRender.Restart();

            }
            framesCurrentSecondRender++;
        }
        public void onResize(ResizeEventArgs eventArgs)
        {
            resolution.X = eventArgs.Width;
            resolution.Y = eventArgs.Height;
        }
        public float getDelta()
        {
            return delta;
        }
        public static void setMouseGrabbed(bool setTo)
        {
            if (setTo)
            {
                gameWindow.CursorState = CursorState.Grabbed;
            } else
            {
                gameWindow.CursorState = CursorState.Normal;
            }
            
        }
    }

}
