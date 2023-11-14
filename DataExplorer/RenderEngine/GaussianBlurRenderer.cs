using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SpaceEngine.Shaders;

namespace SpaceEngine.RenderEngine
{
    internal class GaussianBlurRenderer
    {


        private ShaderProgram vBlurShader = new ShaderProgram("blur_Vertical_Vertex", "blur_Fragment");
        private ShaderProgram hBlurShader = new ShaderProgram("blur_Horizontal_Vertex", "blur_Fragment");

        private static readonly int downSamples = 7;
        private FrameBuffer[] VsampleFramebuffers = new FrameBuffer[downSamples];
        private FrameBuffer[] HsampleFramebuffers = new FrameBuffer[downSamples];
        public GaussianBlurRenderer()
        {
            Vector2i resolution = new Vector2i(WindowHandler.resolution.X, WindowHandler.resolution.Y);

            for (int i = 0; i < downSamples; i++)
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
                VsampleFramebuffers[i] = new FrameBuffer(settings);
                HsampleFramebuffers[i] = new FrameBuffer(settings);

                resolution /= 2;
            }

            vBlurShader.bind();
            vBlurShader.loadUniformInt("originalTexture", 0);
            vBlurShader.unBind();

            hBlurShader.bind();
            hBlurShader.loadUniformInt("originalTexture", 0);
            hBlurShader.unBind();
        }

        public FrameBuffer getRootHBlurFBO()
        {
            return HsampleFramebuffers[0];
        }

        public void renderGaussianBlur(ScreenQuadRenderer renderer, int inputTexture, int downSamples)
        {
            VsampleFramebuffers[0].bind();
            vBlurShader.bind();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, inputTexture);
            for (int i = 0; i<downSamples; i++)
            {



                VsampleFramebuffers[i].bind();
                vBlurShader.bind();
                vBlurShader.loadUniformFloat("targetHeight", VsampleFramebuffers[i].getResolution().Y);
                renderer.render();
                vBlurShader.unBind();


                hBlurShader.bind();
                HsampleFramebuffers[i].bind();
                hBlurShader.loadUniformFloat("targetWidth", HsampleFramebuffers[i].getResolution().X);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, VsampleFramebuffers[i].getRenderAttachment(0));
                renderer.render();
                hBlurShader.unBind();

                vBlurShader.bind();
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, HsampleFramebuffers[i].getRenderAttachment(0));
                vBlurShader.unBind();
            }
            MasterRenderer.simpleShader.bind();
            VsampleFramebuffers[0].bind();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, HsampleFramebuffers[downSamples - 1].getRenderAttachment(0));
            renderer.render();
        }
        public int getLastFinishedBlur()
        {
            return VsampleFramebuffers[0].getRenderAttachment(0);
        }
    }
}
