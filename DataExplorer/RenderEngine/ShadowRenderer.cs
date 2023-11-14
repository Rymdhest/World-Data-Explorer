using OpenTK.Mathematics;
using SpaceEngine.Entity_Component_System.Components;
using SpaceEngine.Entity_Component_System;
using SpaceEngine.Modelling;
using SpaceEngine.Shaders;
using SpaceEngine.Util;
using OpenTK.Graphics.OpenGL;
using SpaceEngine.Entity_Component_System.Systems;

namespace SpaceEngine.RenderEngine
{
    internal class ShadowRenderer
    {
        private ShaderProgram shadowShader = new ShaderProgram("Shadow_Vertex", "Shadow_Fragment");
        public Matrix4 lightViewMatrix;
        private List<ShadowCascade> cascades = new List<ShadowCascade>();

        public ShadowRenderer()
        {
            lightViewMatrix = Matrix4.Identity;

            int multi = 2;
            cascades.Add(new ShadowCascade(new Vector2i(1024, 1024)* multi, 100));
            cascades.Add(new ShadowCascade(new Vector2i(1024, 1024)* multi, 200));
            cascades.Add(new ShadowCascade(new Vector2i(1024, 1024)* multi, 600));
            cascades.Add(new ShadowCascade(new Vector2i(1024, 1024) * multi, 2400));

        }

        public void render(ModelSystem flatShadeEntities, ComponentSystem smoothShadeEntities, Vector3 lightDirection, Entity camera, Matrix4 viewTest, Matrix4 projTest)
        {
            updateLightViewMatrix(-lightDirection, camera.getComponent<Transformation>().position);

            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.PolygonOffsetFill);
            
            shadowShader.bind();

            foreach (ShadowCascade cascade in cascades)
            {
                cascade.bindFrameBuffer();
                GL.Clear(ClearBufferMask.DepthBufferBit);
                GL.PolygonOffset(cascade.getPolygonOffset(), 1f);


                foreach (KeyValuePair<glModel, List<Entity>> glmodels in flatShadeEntities.getModels())
                {
                    glModel glmodel = glmodels.Key;
                    GL.BindVertexArray(glmodel.getVAOID());
                    GL.EnableVertexAttribArray(0);
                    foreach (Entity entity in glmodels.Value)
                    {
                        Matrix4 transformationMatrix = MyMath.createTransformationMatrix(entity.getComponent<Transformation>());
                        shadowShader.loadUniformMatrix4f("modelViewProjectionMatrix", transformationMatrix * lightViewMatrix * cascade.getProjectionMatrix());

                        GL.DrawElements(PrimitiveType.Triangles, glmodel.getVertexCount(), DrawElementsType.UnsignedInt, 0);
                    }
                }



                foreach (Model model in smoothShadeEntities.getMembers())
                {
                    glModel glModel = model.getModel();
                    Matrix4 transformationMatrix = MyMath.createTransformationMatrix(model.owner.getComponent<Transformation>());
                    shadowShader.loadUniformMatrix4f("modelViewProjectionMatrix", transformationMatrix * lightViewMatrix * cascade.getProjectionMatrix());
                    GL.BindVertexArray(glModel.getVAOID());
                    GL.EnableVertexAttribArray(0);

                    GL.DrawElements(PrimitiveType.Triangles, glModel.getVertexCount(), DrawElementsType.UnsignedInt, 0);

                }
            }



            GL.Disable(EnableCap.PolygonOffsetFill);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            shadowShader.unBind();
        }

        public List<ShadowCascade> getShadowCascades()
        {
            return cascades;
        }

        public Matrix4 getLightViewMatrix()
        {
            return lightViewMatrix;
        }

        public int getNumberOfCascades()
        {
            return cascades.Count;
        }

        private Matrix4 updateLightViewMatrix(Vector3 direction, Vector3 center)
        {
            direction.Normalize();
            center *= -1f;
            lightViewMatrix =  Matrix4.Identity;

            float rotX = MathF.Acos((direction.Xz).Length);

            //float rotY = MathF.Atan(direction.X / direction.Z);
            //rotY = direction.Z > 0 ? rotY - MathF.PI : rotY;

            float rotY = MathF.Atan2(direction.X, direction.Z)+MathF.PI;

            lightViewMatrix *= Matrix4.CreateTranslation(new Vector3(center.X, center.Y, center.Z));
            lightViewMatrix *= Matrix4.CreateRotationY(-rotY);
            lightViewMatrix *= Matrix4.CreateRotationX(rotX);
            return lightViewMatrix;
        }
    }
}
