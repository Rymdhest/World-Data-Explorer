using SpaceEngine.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace SpaceEngine.RenderEngine
{
    internal class BloomRenderer
    {
        private ShaderProgram downsamplingShader = new ShaderProgram("Simple_Vertex", "Downsampling_Fragment");
        private ShaderProgram upsamplingShader = new ShaderProgram("Simple_Vertex", "Upsampling_Fragment");
        private ShaderProgram bloomFilterShader = new ShaderProgram("Simple_Vertex", "bloom_Filter_Fragment");
        private static readonly int downSamples = 9;
        private FrameBuffer[] sampleFramebuffers = new FrameBuffer[downSamples];

        public BloomRenderer()
        {
            bloomFilterShader.bind();
            bloomFilterShader.loadUniformInt("shadedInput", 0);
            bloomFilterShader.loadUniformInt("gMaterials", 1);
            bloomFilterShader.unBind();

            downsamplingShader.bind();
            downsamplingShader.loadUniformInt("srcTexture", 0);
            downsamplingShader.unBind();

            upsamplingShader.bind();
            upsamplingShader.loadUniformInt("srcTexture", 0);
            upsamplingShader.loadUniformInt("originalImage", 1);
            upsamplingShader.unBind();

            Vector2i resolution = new Vector2i( WindowHandler.resolution.X, WindowHandler.resolution.Y);

            for (int i = 0; i<downSamples; i++)
            {


                FrameBufferSettings settings = new FrameBufferSettings(resolution);
                DrawBufferSettings drawSettings = new DrawBufferSettings(FramebufferAttachment.ColorAttachment0);
                drawSettings.formatInternal = PixelInternalFormat.Rgba16f;
                //drawSettings.formatExternal = PixelFormat.Rgb;
                drawSettings.pixelType = PixelType.Float;
                drawSettings.wrapMode = TextureWrapMode.ClampToEdge;
                drawSettings.minFilterType = TextureMinFilter.Linear;
                drawSettings.magFilterType = TextureMagFilter.Linear;
                settings.drawBuffers.Add(drawSettings);
                sampleFramebuffers[i] = new FrameBuffer(settings);

                resolution /= 2;
            }
        }

        public void applyBloom(ScreenQuadRenderer renderer, FrameBuffer gBuffer)
        {
            /*
            downsamplingShader.cleanUp();
            upsamplingShader.cleanUp();
            downsamplingShader = new ShaderProgram("Simple_Vertex", "Downsampling_Fragment");
            upsamplingShader = new ShaderProgram("Simple_Vertex", "Upsampling_Fragment");
            downsamplingShader.bind();
            downsamplingShader.loadUniformInt("srcTexture", 0);
            downsamplingShader.unBind();

            upsamplingShader.bind();
            upsamplingShader.loadUniformInt("srcTexture", 0);
            upsamplingShader.loadUniformInt("originalImage", 1);
            upsamplingShader.unBind();
            */
            sampleFramebuffers[0].bind();
            bloomFilterShader.bind();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, renderer.getLastOutputTexture());
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, gBuffer.getRenderAttachment(3));
            renderer.render();
            bloomFilterShader.unBind();
            downsamplingShader.bind();
            for (int i = 0; i< downSamples-1; i++)
            {
                sampleFramebuffers[i+1].bind();
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, sampleFramebuffers[i].getRenderAttachment(0));
                downsamplingShader.loadUniformVector2f("srcResolution", sampleFramebuffers[i].getResolution());
                downsamplingShader.loadUniformInt("mipLevel", i);
                renderer.render(clearColor: true);

            }


            GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            upsamplingShader.bind();
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
            GL.BlendEquation(BlendEquationMode.FuncAdd);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, renderer.getLastOutputTexture());
            for (int i = downSamples-1; i > 0; i--)
            {
                upsamplingShader.loadUniformFloat("filterRadius", 0.0f);
                sampleFramebuffers[i-1].bind();
                upsamplingShader.loadUniformInt("mipLevel", i);
                if (i == 1 )
                {
                    renderer.getNextFrameBuffer().bind();
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, sampleFramebuffers[i].getRenderAttachment(0));
                    renderer.render(blend: false, clearColor: true);
                    //renderer.stepToggle();
                } else
                {
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, sampleFramebuffers[i].getRenderAttachment(0));
                    //upsamplingShader.loadUniformVector2f("srcResolution", sampleFramebuffers[i].getResolution());
                    renderer.render(blend: true, clearColor: false);
                }

            }
            renderer.stepToggle();

            //sampleFramebuffers[1].resolveToScreen();

        }

    }
}
