

using OpenTK.Graphics.OpenGL;
using SpaceEngine.Modelling;
using static SpaceEngine.RenderEngine.MasterRenderer;

namespace SpaceEngine.RenderEngine
{
    internal class glLoader
    {
        public static glModel loadToVAO(Mesh rawModel)
        {
            return loadToVAO(rawModel.getAllPositionsArray(), rawModel.getAllUVsArray(), rawModel.getAllMaterialsArray(), rawModel.getAllNormalsArray(), rawModel.getAllTangentsArray(), rawModel.getAllIndicesArray());
        }


        public static glModel loadToVAO(float[] positions, float[] uvs, float[] materials, float[] normals, float[] tangents, int[] indices)
        {
            int vaoID = createVAO();
            int[] VBOS = new int[6];
            VBOS[5] = bindIndicesBuffer(indices);

            VBOS[0] = storeDataInAttributeList(0, 3, positions);
            VBOS[1] = storeDataInAttributeList(1, 2, uvs);
            VBOS[2] = storeDataInAttributeList(2, 3, materials);
            VBOS[3] = storeDataInAttributeList(3, 3, normals);
            VBOS[4] = storeDataInAttributeList(4, 3, tangents);
            unbindVAO();
            return new glModel(vaoID, VBOS, indices.Length);
        }

        public static glModel loadToVAO(float[] positions, int[] indices, int dimensions)
        {
            int vaoID = createVAO();
            int[] VBOS = new int[2];
            VBOS[1] = bindIndicesBuffer(indices);
            VBOS[0] = storeDataInAttributeList(0, dimensions, positions);
            unbindVAO();
            return new glModel(vaoID, VBOS, indices.Length);
        }

        private static int createVAO()
        {
            int vaoID = GL.GenVertexArray();
            GL.BindVertexArray(vaoID);
            return vaoID;
        }
        private static int bindIndicesBuffer(int[] indices)
        {
            int vboID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticCopy);
            return vboID;
        }
        private static int storeDataInAttributeList(int attributeNumber, int coordinateSize, float[] data)
        {
            int vboID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer,vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(attributeNumber, coordinateSize, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            return vboID;
        }
        private static void unbindVAO()
        {
            GL.BindVertexArray(0);
        }
    }

}
