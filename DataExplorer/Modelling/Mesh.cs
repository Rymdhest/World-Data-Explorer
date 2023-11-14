
using SpaceEngine.RenderEngine;
using OpenTK.Mathematics;
using System.Transactions;
using SpaceEngine.Util;

namespace SpaceEngine.Modelling
{

    public class Vertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector3 tangent;
        public Vector3 material;
        public Vector2 UV;
        public List<Face> faces;
        public int index;
        public Vertex(Vector3 position, int index)
        {
            faces = new List<Face>();
            this.position = position;
            material = new Vector3(0.6f, 0.0f, 0.3f);
            normal = new Vector3(0.0f, 1.0f, 0.0f);
            tangent = new Vector3(1.0f, 0.0f, 0.0f);
            this.index = index;
        }
        public void calculateNormal()
        {
            normal.X = 0;
            normal.Y = 0;
            normal.Z = 0;
            foreach (Face face in faces)
            {
                normal += face.faceNormal;
            }
            normal /= faces.Count;
            normal.Normalize();

            float radius = 1f;
            float u = UV.X;
            float v = UV.Y;
            float x = radius * (float)Math.Sin(u) * (float)Math.Cos(v);
            float y = radius * (float)Math.Sin(u) * (float)Math.Sin(v);
            float z = radius * (float)Math.Cos(u);

            Vector3 result = new Vector3(
                (float)Math.Cos(u) * (float)Math.Cos(v),
                (float)Math.Cos(u) * (float)Math.Sin(v),
                -(float)Math.Sin(u)
            );
            tangent = result;
        }
    }
    public class Face
    {
        public Vertex A;
        public Vertex B;
        public Vertex C;
        public Vector3 faceNormal;

        public Face(Vertex A, Vertex B, Vertex C)
        {
            this.A = A;
            this.B = B;
            this.C = C;
            faceNormal = new Vector3(0.0f, 1.0f, 0.0f);


        }
        public void calcFaceNormal()
        {
            faceNormal = Vector3.Cross((B.position - A.position), (C.position - A.position));
            faceNormal.Normalize();
        }

    }

    internal class Mesh
    {
        public List<Face> faces;
        public List<Vertex> vertices;


        public Mesh(List<Face> faces, List<Vertex> vertices)
        {
            this.faces = faces;
            this.vertices = vertices;
        }
        public Mesh(float[] positions, float[] uvs, int[] indices)
        {
            vertices = new List<Vertex>();
            faces = new List<Face>();

            for (int i = 0; i <positions.Length/3; i++)
            {
                Vertex v = new Vertex(new Vector3(positions[i*3], positions[i*3+1], positions[i*3+2]), i);
                v.UV = new Vector2(uvs[i*2], uvs[i*2+1]);
                vertices.Add(v);
            }
            for (int i = 0; i < indices.Length / 3; i++)
            {
                Vertex A = vertices[indices[i * 3]];
                Vertex B = vertices[indices[i * 3 + 1]];
                Vertex C = vertices[indices[i * 3 + 2]];
                Face face = new Face(A, B, C);
                A.faces.Add(face);
                B.faces.Add(face);
                C.faces.Add(face);
                faces.Add(face);
            }
            calculateAllNormals();
        }

        public void calculateAllNormals()
        {
            foreach (Face face in faces) {
                face.calcFaceNormal();
            }
            foreach (Vertex vertex in vertices)
            {
                vertex.calculateNormal();
            }
        }

        public void setRoughness(float setTo)
        {
            foreach (Vertex vertex in vertices)
            {
                vertex.material.X = setTo;
            }
        }
        public void setEmission(float setTo)
        {
            foreach (Vertex vertex in vertices)
            {
                vertex.material.Y = setTo;
            }
        }
        public void setMetalicness(float setTo)
        {
            foreach (Vertex vertex in vertices)
            {
                vertex.material.Z = setTo;
            }
        }

        public float[] getAllPositionsArray()
        {
            float[] positionsArray = new float[vertices.Count*3];

            for (int i = 0; i < vertices.Count; i++)
            {
                positionsArray[3 * i + 0] = vertices[i].position.X;
                positionsArray[3 * i + 1] = vertices[i].position.Y;
                positionsArray[3 * i + 2] = vertices[i].position.Z;
            }
            return positionsArray;
        }

        public float[] getAllNormalsArray()
        {
            float[] normalsArray = new float[vertices.Count * 3];

            for (int i = 0; i < vertices.Count; i++)
            {
                normalsArray[3 * i + 0] = vertices[i].normal.X;
                normalsArray[3 * i + 1] = vertices[i].normal.Y;
                normalsArray[3 * i + 2] = vertices[i].normal.Z;
            }
            return normalsArray;
        }

        public float[] getAllTangentsArray()
        {
            float[] tangentsArray = new float[vertices.Count * 3];

            for (int i = 0; i < vertices.Count; i++)
            {
                tangentsArray[3 * i + 0] = vertices[i].tangent.X;
                tangentsArray[3 * i + 1] = vertices[i].tangent.Y;
                tangentsArray[3 * i + 2] = vertices[i].tangent.Z;
            }
            return tangentsArray;
        }

        public float[] getAllMaterialsArray()
        {
            float[] materialsArray = new float[vertices.Count * 3];

            for (int i = 0; i < vertices.Count; i++)
            {
                materialsArray[3 * i + 0] = vertices[i].material.X;
                materialsArray[3 * i + 1] = vertices[i].material.Y;
                materialsArray[3 * i + 2] = vertices[i].material.Z;
            }
            return materialsArray;
        }

        public float[] getAllUVsArray()
        {
            float[] UVsArray = new float[vertices.Count * 2];

            for (int i = 0; i < vertices.Count; i++)
            {
                UVsArray[2 * i + 0] = vertices[i].UV.X;
                UVsArray[2 * i + 1] = vertices[i].UV.Y;
            }
            return UVsArray;
        }

        public int[] getAllIndicesArray()
        {
            int indicesCount = faces.Count * 3;
            int[] indicesArray = new int[indicesCount];

            for (int i = 0; i < faces.Count; i++)
            {
                indicesArray[3*i+0] = faces[i].A.index;
                indicesArray[3*i+1] = faces[i].B.index;
                indicesArray[3*i+2] = faces[i].C.index;
            }
            return indicesArray;
        }
        public void cleanUp()
        {

        }
    }
}
