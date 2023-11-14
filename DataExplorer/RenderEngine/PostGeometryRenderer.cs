using OpenTK.Mathematics;
using SpaceEngine.Entity_Component_System;
using SpaceEngine.Entity_Component_System.Components;
using SpaceEngine.Shaders;
using SpaceEngine.Util;
using OpenTK.Graphics.OpenGL;
using SpaceEngine.Modelling;

namespace SpaceEngine.RenderEngine
{
    internal class PostGeometryRenderer
    {

        private ShaderProgram postGeometryShader = new ShaderProgram("Post_Geometry_Vertex", "Post_Geometry_Fragment");
        public void render(ComponentSystem postGeometrySystem, Matrix4 viewMatrix, Matrix4 projectionMatrix)
        {
            //postGeometryShader.cleanUp();
            //postGeometryShader = new ShaderProgram("Post_Geometry_Vertex", "Post_Geometry_Fragment");
            prepareFrame();
            postGeometryShader.bind();

            glModel glModel = ModelGenerator.unitSphere;
            GL.BindVertexArray(glModel.getVAOID());
            GL.EnableVertexAttribArray(0);
            postGeometryShader.loadUniformMatrix4f("projectionMatrix", projectionMatrix);
            postGeometryShader.loadUniformVector2f("screenResolution", WindowHandler.resolution);
            foreach (GlowEffect glowEffect in postGeometrySystem.getMembers())
            {
                Matrix4 transformationMatrix = MyMath.createTransformationMatrix(glowEffect.owner.getComponent<Transformation>().position, glowEffect.glowRadius);
                Matrix4 modelViewMatrix = transformationMatrix * viewMatrix;
                postGeometryShader.loadUniformMatrix4f("modelViewMatrix", modelViewMatrix);
                postGeometryShader.loadUniformMatrix4f("modelViewProjectionMatrix", modelViewMatrix * projectionMatrix);
                Vector3 pos = glowEffect.owner.getComponent<Transformation>().position;
                postGeometryShader.loadUniformVector4f("modelWorldPosition", (new Vector4(pos.X, pos.Y, pos.Z, 1.0f)* viewMatrix* projectionMatrix));
                
                postGeometryShader.loadUniformVector3f("color", glowEffect.color);
                postGeometryShader.loadUniformFloat("scale", glowEffect.owner.getComponent<Transformation>().scale);
                //postGeometryShader.loadUniformMatrix4f("normalModelViewMatrix", Matrix4.Transpose(Matrix4.Invert(modelViewMatrix)));

                //GL.EnableVertexAttribArray(1);
                //GL.EnableVertexAttribArray(2);
                //GL.EnableVertexAttribArray(3);

                GL.DrawElements(PrimitiveType.Triangles, glModel.getVertexCount(), DrawElementsType.UnsignedInt, 0);

            }
            postGeometryShader.unBind();
            finishFrame();
        }
        private void prepareFrame()
        {
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.Blend);
        }
        private void finishFrame()
        {
            GL.BindVertexArray(0);
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);
            GL.CullFace(CullFaceMode.Back);
            GL.Disable(EnableCap.Blend);
            //GL.DisableVertexAttribArray(3);
        }
    }
}
