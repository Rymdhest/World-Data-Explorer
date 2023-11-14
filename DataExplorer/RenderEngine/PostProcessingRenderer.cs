using SpaceEngine.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using SpaceEngine.Entity_Component_System;
using SpaceEngine.Entity_Component_System.Components;
using System.Diagnostics;
using SpaceEngine.Modelling;
using SpaceEngine.Util;

namespace SpaceEngine.RenderEngine
{
    internal class PostProcessingRenderer
    {

        private ShaderProgram FXAAShader = new ShaderProgram("Simple_Vertex", "FXAA_Fragment");

        private ShaderProgram combineShader = new ShaderProgram("Simple_Vertex", "Combine_Fragment");
        private ShaderProgram skyShader = new ShaderProgram("Simple_Vertex", "sky_Fragment");
        private ShaderProgram HDRMapShader = new ShaderProgram("Simple_Vertex", "HDR_Mapper_Fragment");
        private ShaderProgram ScreenSpaceReflectionShader = new ShaderProgram("Simple_Vertex", "Screen_Reflection_Fragment");
        private ShaderProgram combineReflectionShader = new ShaderProgram("Simple_Vertex", "Combine_Reflection_Fragment");

        private PostGeometryRenderer postGeometryRenderer;
        private FrameBuffer bloomFilterFBO;

        private BloomRenderer bloomRenderer;
        private GaussianBlurRenderer gaussianBlurRenderer;

        public PostProcessingRenderer()
        {

            FXAAShader.bind();
            FXAAShader.loadUniformInt("l_tex", 0);
            FXAAShader.unBind();

            HDRMapShader.bind();
            HDRMapShader.loadUniformInt("HDRcolorTexture", 0);
            HDRMapShader.unBind();


            combineShader.bind();
            combineShader.loadUniformInt("texture0", 0);
            combineShader.loadUniformInt("texture1", 1);
            combineShader.unBind();

            combineReflectionShader.bind();
            combineReflectionShader.loadUniformInt("sourceColorTexture", 0);
            combineReflectionShader.loadUniformInt("reflectionTexture", 1);
            combineReflectionShader.loadUniformInt("gMaterials", 2);
            combineReflectionShader.unBind();

            ScreenSpaceReflectionShader.bind();
            ScreenSpaceReflectionShader.loadUniformInt("shadedColor", 0);
            ScreenSpaceReflectionShader.loadUniformInt("gNormal", 1);
            ScreenSpaceReflectionShader.loadUniformInt("gPosition", 2);
            ScreenSpaceReflectionShader.loadUniformInt("gMaterials", 3);
            ScreenSpaceReflectionShader.unBind();


            FrameBufferSettings bloomFrameBufferSettings = new FrameBufferSettings(WindowHandler.resolution);
            bloomFrameBufferSettings.drawBuffers.Add(new DrawBufferSettings(FramebufferAttachment.ColorAttachment0));
            bloomFilterFBO = new FrameBuffer(bloomFrameBufferSettings);

            bloomRenderer = new BloomRenderer();
            gaussianBlurRenderer = new GaussianBlurRenderer();
            postGeometryRenderer = new PostGeometryRenderer();


            



        }



        public void doPostProcessing(ScreenQuadRenderer renderer, FrameBuffer gBuffer, Entity sunEntity, Vector3 viewPosition, Matrix4 viewMatrix, Matrix4 projectionMatrix)
        {
            applySky(renderer, gBuffer, sunEntity, viewPosition, viewMatrix, projectionMatrix);
            postGeometryRenderer.render(EntityManager.postGeometrySystem, viewMatrix, projectionMatrix);


            bloomRenderer.applyBloom(renderer, gBuffer);
            //GL.Finish();
            //Stopwatch stopwatch = Stopwatch.StartNew();
            //applyScreenSpaceReflections(renderer, gBuffer, projectionMatrix, sunEntity.getComponent<Sun>());
            //GL.Finish();
            //Console.WriteLine(stopwatch.Elapsed.TotalMilliseconds);

            HDRMap(renderer);
            applyFXAA(renderer);
        }

        private void applySky(ScreenQuadRenderer renderer, FrameBuffer gBuffer, Entity sunEntity, Vector3 viewPosition, Matrix4 viewMatrix, Matrix4 projectionMatrix)
        {
            //renderer.getNextFrameBuffer().blitDepthBufferFrom(gBuffer);
            //renderer.getLastFrameBuffer().blitDepthBufferFrom(gBuffer);
            /*
            skyShader.cleanUp();
            skyShader = new ShaderProgram("Simple_Vertex", "sky_Fragment");
            */
            Sun sun = sunEntity.getComponent<Sun>();



            skyShader.bind();
            skyShader.loadUniformVector3f("viewPositionWorld", viewPosition);
            skyShader.loadUniformMatrix4f("viewMatrix", viewMatrix);
            skyShader.loadUniformMatrix4f("projectionMatrix", projectionMatrix);
            
            skyShader.loadUniformVector2f("screenResolution", WindowHandler.resolution);
            //Vector4 sunDirectionViewSpace = new Vector4(sunDirection.X, sunDirection.Y, sunDirection.Z, 1.0f) * Matrix4.Transpose(Matrix4.Invert(viewMatrix));
            skyShader.loadUniformVector3f("sunDirectionWorldSpace", sun.getDirection());

            skyShader.loadUniformVector3f("skyColorGround", sun.getSkyColorGround());
            skyShader.loadUniformVector3f("skyColorSpace", sun.getSkyColorSpace());
            skyShader.loadUniformVector3f("sunColorGlare", sun.getSunScatterColor());
            skyShader.loadUniformVector3f("sunColor", sun.getFullSunColor());
            skyShader.loadUniformFloat("sunSetFactor", sun.getSunsetFactor());


            renderer.getLastFrameBuffer().bind();

            GL.DepthFunc(DepthFunction.Lequal);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, renderer.getLastOutputTexture());

            //GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            renderer.render(depthTest :true, depthMask:false, blend :false, clearColor:false);
            //renderer.stepToggle();
            skyShader.unBind();

            GL.DepthFunc(DepthFunction.Less);
        } 

        private void applyScreenSpaceReflections(ScreenQuadRenderer renderer, FrameBuffer gBuffer, Matrix4 projectionMatrix, Sun sun)
        {
            /*
            ScreenSpaceReflectionShader.cleanUp();
            ScreenSpaceReflectionShader = new ShaderProgram("Simple_Vertex", "Screen_Reflection_Fragment");

            ScreenSpaceReflectionShader.bind();
            ScreenSpaceReflectionShader.loadUniformInt("shadedColor", 0);
            ScreenSpaceReflectionShader.loadUniformInt("gNormal", 1);
            ScreenSpaceReflectionShader.loadUniformInt("gPosition", 2);
            ScreenSpaceReflectionShader.loadUniformInt("gMaterials", 3);
            ScreenSpaceReflectionShader.unBind();
            */


            ScreenSpaceReflectionShader.bind();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, renderer.getLastOutputTexture());
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, gBuffer.getRenderAttachment(1));
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, gBuffer.getRenderAttachment(2));
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, gBuffer.getRenderAttachment(3));
            ScreenSpaceReflectionShader.loadUniformMatrix4f("projectionMatrix", projectionMatrix);
            ScreenSpaceReflectionShader.loadUniformVector3f("skyColor", sun.getSkyColorGround());
            gaussianBlurRenderer.getRootHBlurFBO().bind();
            renderer.render();
            ScreenSpaceReflectionShader.unBind();

            gaussianBlurRenderer.renderGaussianBlur(renderer, gaussianBlurRenderer.getRootHBlurFBO().getRenderAttachment(0), 1);

            combineReflectionShader.bind();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, renderer.getLastOutputTexture());
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, gaussianBlurRenderer.getLastFinishedBlur());
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, gBuffer.getRenderAttachment(3));
            renderer.renderToNextFrameBuffer();
        }

        private void HDRMap(ScreenQuadRenderer renderer)
        {
            
            HDRMapShader.cleanUp();
            HDRMapShader = new ShaderProgram("Simple_Vertex", "HDR_Mapper_Fragment");
            
            HDRMapShader.bind();
            renderer.renderTextureToNextFrameBuffer(renderer.getLastOutputTexture());
            HDRMapShader.unBind();
        }

        private void applyFXAA(ScreenQuadRenderer renderer)
        {
            FXAAShader.bind();
            FXAAShader.loadUniformVector2f("win_size", WindowHandler.resolution);
            renderer.renderTextureToNextFrameBuffer(renderer.getLastOutputTexture());
            FXAAShader.unBind();
        }

        public void onResize(ResizeEventArgs eventArgs)
        {
            bloomFilterFBO.resize(WindowHandler.resolution);
        }
    }
}
