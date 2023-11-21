
using OpenTK.Mathematics;
using SpaceEngine.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using SpaceEngine.Entity_Component_System;
using SpaceEngine.Core;
using SpaceEngine.Entity_Component_System.Components;
using SpaceEngine.Modelling;
using System.Diagnostics;
using DataExplorer.Modelling;
using DataExplorer.DataEarth;

namespace SpaceEngine.RenderEngine
{
    internal class MasterRenderer
    {
        public enum Pipeline {FLAT_SHADING, SMOOTH_SHADING, POST_GEOMETRY, OTHER};
        public static ShaderProgram simpleShader = new ShaderProgram("Simple_Vertex", "Simple_Fragment");
        private Matrix4 projectionMatrix;
        public static float fieldOfView;
        public static float near = 0.1f;
        public static float far = 1000f;
        private ScreenQuadRenderer screenQuadRenderer;
        private GeometryPassRenderer geometryPassRenderer;
        private DeferredLightPassRenderer deferredLightPassRenderer;
        private PostProcessingRenderer postProcessingRenderer;
        private ShadowRenderer shadowRenderer;
        private TextureMaster textureMaster = new TextureMaster();
        public MasterRenderer() {
            fieldOfView = MathF.PI/2.5f;

            screenQuadRenderer = new ScreenQuadRenderer();
            geometryPassRenderer = new GeometryPassRenderer();
            deferredLightPassRenderer= new DeferredLightPassRenderer();
            postProcessingRenderer= new PostProcessingRenderer();
            shadowRenderer = new ShadowRenderer();
            updateProjectionMatrix();
        }
        private void updateProjectionMatrix()
        {

            float aspect = (float)WindowHandler.resolution.X / (float)WindowHandler.resolution.Y;
            float y_scale = (float)((1f / Math.Tan((fieldOfView / 2f))));
            float x_scale = y_scale / aspect;
            float frustum_length = far - near;
            
            projectionMatrix = Matrix4.Identity;
            projectionMatrix.M11 = x_scale;
            projectionMatrix.M22 = y_scale;
            projectionMatrix.M33 = -((far + near) / frustum_length);
            projectionMatrix.M34 = -1f;
            projectionMatrix.M43 = -((2 * near * far) / frustum_length);
            //projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(fieldOfView, aspect, near, far);
        }

        public void prepareFrame()
        {


        }

        public void finishFrame()
        {



            WindowHandler.getWindow().SwapBuffers();
        }
   
        public void render(Matrix4 viewMatrix, Entity camera, Entity sunEntity, ComponentSystem pointLights)
        {
            prepareFrame();


            //GL.Finish();
            //Stopwatch stopwatch = Stopwatch.StartNew();

            geometryPassRenderer.render(EntityManager.flatShadingSystem, EntityManager.smoothShadingSystem, viewMatrix, projectionMatrix, camera.getComponent<Transformation>().position);
            //GL.Finish();
            //Console.WriteLine(stopwatch.ElapsedMilliseconds);

            shadowRenderer.render(EntityManager.flatShadingSystem, EntityManager.smoothShadingSystem, sunEntity.getComponent<Sun>().getDirection(), camera, viewMatrix, projectionMatrix);

         
            deferredLightPassRenderer.render(screenQuadRenderer, geometryPassRenderer.gBuffer, sunEntity, viewMatrix,projectionMatrix, pointLights, shadowRenderer);

            postProcessingRenderer.doPostProcessing(screenQuadRenderer, geometryPassRenderer.gBuffer, sunEntity, camera.getComponent<Transformation>().position, viewMatrix, projectionMatrix);
           simpleShader.bind();
            simpleShader.loadUniformInt("blitTexture", 0);
            screenQuadRenderer.renderTextureToScreen(screenQuadRenderer.getLastOutputTexture());
            //screenQuadRenderer.renderTextureToScreen(geometryPassRenderer.gBuffer.getRenderAttachment(3));
            //screenQuadRenderer.renderTextureToScreen(Engine.earth.dataFrameBuffer.getRenderAttachment(0));
            simpleShader.unBind();




            finishFrame();
        }
        public void update(float delta)
        {

        }
        public void onResize(ResizeEventArgs eventArgs)
        {
            screenQuadRenderer.onResize(eventArgs);
            geometryPassRenderer.onResize(eventArgs);
            postProcessingRenderer.onResize(eventArgs);
            GL.Viewport(0, 0, WindowHandler.resolution.X, WindowHandler.resolution.Y);
            updateProjectionMatrix();
        }
    }
}
