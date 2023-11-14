
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SpaceEngine.RenderEngine;

namespace SpaceEngine.RenderEngine
{
    internal class FrameBufferSettings
    {
        public DepthAttachmentSettings? depthAttachmentSettings = null;
        public List<DrawBufferSettings> drawBuffers= new List<DrawBufferSettings>();
        public Vector2i resolution;
        public FrameBufferSettings(Vector2i resolution) {
            this.resolution = resolution;
        }
    }
}
