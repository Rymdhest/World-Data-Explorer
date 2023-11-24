using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using SpaceEngine.Modelling;
namespace SpaceEngine.RenderEngine
{
    internal class ScreenQuadRenderer
    {
        private FrameBuffer buffer1;
        private FrameBuffer buffer2;
        private bool toggle;
        private static float[] positions = { -1, 1, -1, -1, 1, -1, 1, 1 };
        private static int[] indices = { 0, 1, 2, 3, 0, 2 };
        public static glModel quadModel = glLoader.loadToVAO(positions, indices, 2);
        public ScreenQuadRenderer() {


            FrameBufferSettings frameBufferSettings= new FrameBufferSettings(WindowHandler.resolution);
            frameBufferSettings.drawBuffers.Add(new DrawBufferSettings(FramebufferAttachment.ColorAttachment0));
            DepthAttachmentSettings depthSettings = new DepthAttachmentSettings();
            depthSettings.isTexture = true;
            frameBufferSettings.depthAttachmentSettings = depthSettings;
            buffer1 = new FrameBuffer(frameBufferSettings);
            buffer2 = new FrameBuffer(frameBufferSettings);
            toggle = true;
        }
        private void renderTexture(int texture)
        {

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            render();
        }
        public void renderToScreen()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            render();
        }
        public void renderTextureToScreen(int texture)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            renderTexture(texture);
        }
        public static void render(bool depthTest = false, bool depthMask = false, bool blend = false, bool clearColor = true)
        {
            GL.BindVertexArray(quadModel.getVAOID());
            GL.EnableVertexAttribArray(0);

            GL.ClearColor(0f, 0f, 1f, 1f);
            if (clearColor)GL.Clear(ClearBufferMask.ColorBufferBit);
            

            if (depthTest) GL.Enable(EnableCap.DepthTest); 
            else GL.Disable(EnableCap.DepthTest);

            GL.DepthMask(depthMask);

            if (blend) GL.Enable(EnableCap.Blend);
            else GL.Disable(EnableCap.Blend);

            GL.DrawElements(PrimitiveType.Triangles, quadModel.getVertexCount(), DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
        }

        public void renderToNextFrameBuffer()
        {
            getNextFrameBuffer().bind();
            render();
            stepToggle();
        }

        public void renderTextureToNextFrameBuffer(int texture)
        {
            getNextFrameBuffer().bind();
            //GL.Viewport(0, 0, resolution.X, resolution.Y);
            renderTexture(texture);
            stepToggle();
        }
        public int getLastOutputTexture()
        {
            return getLastFrameBuffer().getRenderAttachment(0);
        }
        public int getNextOutputTexture()
        {
            return getNextFrameBuffer().getRenderAttachment(0);
        }
        public FrameBuffer getNextFrameBuffer()
        {
            if (toggle) return buffer1;
            else return buffer2;
        }
        public FrameBuffer getLastFrameBuffer()
        {
            if (toggle) return buffer2;
            else return buffer1;
        }
        public void stepToggle()
        {
            if (toggle == true) toggle = false;
            else toggle = true;
        }
        public void onResize(ResizeEventArgs eventArgs)
        {
            buffer1.resize(WindowHandler.resolution);
            buffer2.resize(WindowHandler.resolution);
        }
    }
}
