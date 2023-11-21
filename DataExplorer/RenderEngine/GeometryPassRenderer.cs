using OpenTK.Mathematics;
using SpaceEngine.Entity_Component_System.Components;
using SpaceEngine.Entity_Component_System;
using SpaceEngine.Util;
using OpenTK.Graphics.OpenGL;
using SpaceEngine.Shaders;
using OpenTK.Windowing.Common;
using SpaceEngine.Modelling;
using SpaceEngine.Entity_Component_System.Systems;
using System.Reflection;
using DataExplorer.Modelling;
using SpaceEngine.Core;

namespace SpaceEngine.RenderEngine
{
    internal class GeometryPassRenderer
    {
        private ShaderProgram flatShader = new ShaderProgram("Flat_Shade_Vertex", "Flat_Shade_Fragment", "Flat_Shade_Geometry");
        private ShaderProgram earthShader = new ShaderProgram("Earth_Shade_Vertex", "Earth_Shade_Fragment");
        public FrameBuffer gBuffer;

        public GeometryPassRenderer()
        {
            FrameBufferSettings gBufferSettings = new FrameBufferSettings(WindowHandler.resolution);
            DrawBufferSettings gAlbedo = new DrawBufferSettings(FramebufferAttachment.ColorAttachment0);
            gAlbedo.formatInternal = PixelInternalFormat.Rgba16f;
            gBufferSettings.drawBuffers.Add(gAlbedo);

            DrawBufferSettings gNormal = new DrawBufferSettings(FramebufferAttachment.ColorAttachment1);
            gNormal.formatInternal = PixelInternalFormat.Rgba16f;
            gNormal.pixelType = PixelType.Float;
            gBufferSettings.drawBuffers.Add(gNormal);

            DrawBufferSettings gPosition = new DrawBufferSettings(FramebufferAttachment.ColorAttachment2);
            gPosition.formatInternal = PixelInternalFormat.Rgba16f;
            gPosition.pixelType = PixelType.Float;
            gBufferSettings.drawBuffers.Add(gPosition);

            
            DrawBufferSettings gMaterials = new DrawBufferSettings(FramebufferAttachment.ColorAttachment3);
            gMaterials.formatInternal = PixelInternalFormat.Rgba16f;
            gMaterials.pixelType = PixelType.Float;
            gBufferSettings.drawBuffers.Add(gMaterials);
            

            DepthAttachmentSettings depthSettings = new DepthAttachmentSettings();
            depthSettings.isTexture = true;
            gBufferSettings.depthAttachmentSettings = depthSettings;
            gBuffer = new FrameBuffer(gBufferSettings);


            earthShader.bind();
            earthShader.loadUniformInt("albedoTexture", 0);
            earthShader.loadUniformInt("topographyTexture", 1);
            earthShader.loadUniformInt("countryDataTexture", 2);
            earthShader.unBind();




        }
        private void prepareFrame(Matrix4 viewMatrix, Matrix4 projectionMatrix)
        {
            gBuffer.bind();
            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.Disable(EnableCap.Blend);
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.2f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        private void renderEarth(ComponentSystem smoothShadeEntities, Matrix4 viewMatrix, Matrix4 projectionMatrix)
        {

            Engine.earth.countryHighlights[1] = 0;
            Engine.earth.countryHighlights[2] = 0;
            Engine.earth.countryHighlights[3] = 1;

            earthShader.bind();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, TextureMaster.earthAlbedo.textureID);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, TextureMaster.earthTopography.textureID);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, Engine.earth.countriesDataTexture.textureID);
            earthShader.loadUniformVector2f("heightmapSize", TextureMaster.earthTopography.resolution);
            foreach (Model model in smoothShadeEntities.getMembers())
            {
                glModel glModel = model.getModel();

                Matrix4 transformationMatrix = MyMath.createTransformationMatrix(model.owner.getComponent<Transformation>());
                Matrix4 modelViewMatrix = transformationMatrix * viewMatrix;
                earthShader.loadUniformMatrix4f("modelViewMatrix", modelViewMatrix);
                earthShader.loadUniformMatrix4f("modelViewProjectionMatrix", modelViewMatrix * projectionMatrix);
                earthShader.loadUniformMatrix4f("normalModelViewMatrix", Matrix4.Transpose(Matrix4.Invert(modelViewMatrix)));
                earthShader.loadUniformIntArray("countryHighlights", Engine.earth.countryHighlights);
                GL.BindVertexArray(glModel.getVAOID());
                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);
                GL.EnableVertexAttribArray(2);
                GL.EnableVertexAttribArray(3);
                GL.EnableVertexAttribArray(4);

                GL.DrawElements(PrimitiveType.Triangles, glModel.getVertexCount(), DrawElementsType.UnsignedInt, 0);

            }
            earthShader.unBind();
        }

        public void render(ModelSystem flatShadeEntities, ComponentSystem smoothShadeEntities, Matrix4 viewMatrix, Matrix4 projectionMatrix, Vector3 cameraPosition)
        {

            prepareFrame(viewMatrix, projectionMatrix);
            flatShader.bind();

            foreach (KeyValuePair<glModel, List<Entity>> glmodels in flatShadeEntities.getModels()) {
                glModel glmodel = glmodels.Key;

                GL.BindVertexArray(glmodel.getVAOID());
                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);
                GL.EnableVertexAttribArray(2);
                earthShader.loadUniformMatrix4f("viewMatrix", viewMatrix);
                foreach (Entity entity in glmodels.Value)
                {
                    Matrix4 transformationMatrix = MyMath.createTransformationMatrix(entity.getComponent<Transformation>());
                    Matrix4 modelViewMatrix = transformationMatrix * viewMatrix;
                    earthShader.loadUniformMatrix4f("modelViewMatrix", modelViewMatrix);
                    earthShader.loadUniformMatrix4f("modelViewProjectionMatrix", modelViewMatrix * projectionMatrix);

                    GL.DrawElements(PrimitiveType.Triangles, glmodel.getVertexCount(), DrawElementsType.UnsignedInt, 0);
                }
            }
            flatShader.unBind();
            
            renderEarth(smoothShadeEntities,viewMatrix, projectionMatrix);

            //GL.BindBuffer(BufferTarget.ArrayBuffer, model.getIndexBuffer());
            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            finishFrame();
        }
        public void onResize(ResizeEventArgs eventArgs)
        {
            gBuffer.resize(WindowHandler.resolution);
        }
        private void finishFrame()
        {
            GL.BindVertexArray(0);
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);
            GL.DisableVertexAttribArray(3);
        }
    }
}
