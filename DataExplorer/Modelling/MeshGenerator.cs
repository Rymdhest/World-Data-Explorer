
using DataExplorer.Modelling;
using OpenTK.Mathematics;
using SpaceEngine.Entity_Component_System.Components;
using SpaceEngine.Entity_Component_System;
using SpaceEngine.RenderEngine;
using SpaceEngine.Util;
using System;
using System.Drawing;
using static OpenTK.Graphics.OpenGL.GL;

namespace SpaceEngine.Modelling
{
    internal class MeshGenerator
    {
        public static Mesh generatePlane(Vector2 size)
        {
            float[] positions = {
               -size.X, -size.Y, 0,
                size.X, -size.Y, 0,
                size.X,  size.Y, 0,
               -size.X,  size.Y, 0};

            float[] normals = {
               0, 0, 1,
                0, 0, 1,
                0,  0, 1,
               0,  0, 1};

            float[] uvs = {
               0, 0,
                1, 0,
                1,  1,
               0,  1};

            int[] indices = {
               0, 1, 2, 3, 0, 2};

            return new Mesh(positions, uvs, indices);
        }

        public static Mesh CreateIcosphere(int order)
        {
            // set up a 20-triangle icosahedron
            float f = (1 + (float)Math.Sqrt(5)) / 2;
            int T = (int)Math.Pow(4, order);

            float[] positions = new float[(10 * T + 2) * 3];
            float[] uvs = new float[(10 * T + 2) * 2];
            Array.Copy(new float[]
            {
            -1, f, 0, 1, f, 0, -1, -f, 0, 1, -f, 0,
            0, -1, f, 0, 1, f, 0, -1, -f, 0, 1, -f,
            f, 0, -1, f, 0, 1, -f, 0, -1, -f, 0, 1
            }, positions, 36);

            int[] triangles = new int[]
            {
            0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
            11, 10, 2, 5, 11, 4, 1, 5, 9, 7, 1, 8, 10, 7, 6,
            3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
            9, 8, 1, 4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7
            };

            int vert = 12;
            Dictionary<int, int>? midCache = order > 0 ? new Dictionary<int, int>() : null; // midpoint vertices cache to avoid duplicating shared vertices

            int addMidPoint(int a, int b)
            {
                int key = (int)((a + b) * (a + b + 1) / 2) + Math.Min(a, b); // Cantor's pairing function
                if (midCache.TryGetValue(key, out int i))
                {
                    midCache.Remove(key);
                    return i;
                }
                midCache[key] = vert;
                for (int k = 0; k < 3; k++)
                {
                    positions[3 * vert + k] = (positions[3 * a + k] + positions[3 * b + k]) / 2;
                }
                i = vert++;
                return i;
            }

            int[] trianglesPrev = triangles;
            for (int i = 0; i < order; i++)
            {
                // subdivide each triangle into 4 triangles
                triangles = new int[trianglesPrev.Length * 4];
                for (int k = 0; k < trianglesPrev.Length; k += 3)
                {
                    int v1 = trianglesPrev[k + 0];
                    int v2 = trianglesPrev[k + 1];
                    int v3 = trianglesPrev[k + 2];
                    int a = addMidPoint(v1, v2);
                    int b = addMidPoint(v2, v3);
                    int c = addMidPoint(v3, v1);
                    int t = k * 4;
                    triangles[t++] = v1; triangles[t++] = a; triangles[t++] = c;
                    triangles[t++] = v2; triangles[t++] = b; triangles[t++] = a;
                    triangles[t++] = v3; triangles[t++] = c; triangles[t++] = b;
                    triangles[t++] = a; triangles[t++] = b; triangles[t++] = c;
                }
                trianglesPrev = triangles;
            }

            // normalize vertices
            for (int i = 0; i < positions.Length; i += 3)
            {
                float m = 1 / (float)Math.Sqrt(positions[i] * positions[i] + positions[i + 1] * positions[i + 1] + positions[i + 2] * positions[i + 2]);
                positions[i] *= m;
                positions[i + 1] *= m;
                positions[i + 2] *= m;
            }
            for (int i = 0; i < positions.Length; i += 3)
            {
                float x = positions[i];
                float y = positions[i + 1];
                float z = positions[i + 2];

                float u = ((MathF.Atan2(x, z) / (2 * MathF.PI))+0.5f);
                float v = (MathF.Asin(y) / MathF.PI)+0.5f;

                uvs[i / 3 * 2] = u;
                uvs[i / 3 * 2 + 1] = v;
            }



            return new Mesh(positions, uvs, triangles);
        }

        public static Mesh generateEarth()
        {
            Mesh mesh = CreateIcosphere(5);


            Bitmap heightMap = new Bitmap("Textures\\" + TextureMaster.earthTopography.name + ".png");
            Bitmap AlbedoMap = new Bitmap("Textures\\" + TextureMaster.earthAlbedo.name + ".png");
            foreach (Vertex vertex in mesh.vertices)
            {

                int pixelX = Math.Clamp((int)(vertex.UV.X * TextureMaster.earthTopography.resolution.X), 0, TextureMaster.earthTopography.resolution.X - 1);
                int pixelY = Math.Clamp((int)(vertex.UV.Y * TextureMaster.earthTopography.resolution.Y), 0, TextureMaster.earthTopography.resolution.Y - 1);

                float heightScale = 1f + 0.00012f * Convert.ToInt32(heightMap.GetPixel(pixelX, TextureMaster.earthTopography.resolution.Y - pixelY - 1).R);
                //heightScale = 1f;
                vertex.position.X *= heightScale;
                vertex.position.Y *= heightScale;
                vertex.position.Z *= heightScale;

                vertex.material.Y = 0f;
                vertex.material.Z = 0f;
                vertex.material.X = 0.6f;


                pixelX = Math.Clamp((int)(vertex.UV.X * TextureMaster.earthAlbedo.resolution.X), 0, TextureMaster.earthAlbedo.resolution.X - 1);
                pixelY = Math.Clamp((int)(vertex.UV.Y * TextureMaster.earthAlbedo.resolution.Y), 0, TextureMaster.earthAlbedo.resolution.Y - 1);

                int R = Convert.ToInt32(AlbedoMap.GetPixel(pixelX, TextureMaster.earthAlbedo.resolution.Y - pixelY - 1).R);
                int G = Convert.ToInt32(AlbedoMap.GetPixel(pixelX, TextureMaster.earthAlbedo.resolution.Y - pixelY - 1).G);
                int B = Convert.ToInt32(AlbedoMap.GetPixel(pixelX, TextureMaster.earthAlbedo.resolution.Y - pixelY - 1).B);

                if (Math.Max(R, G) < B)
                {
                    vertex.material.X = 0.1f;
                    vertex.material.Z = 0.3f;
                }
                Entity ball = new Entity();

            }

            mesh.calculateAllNormals();
            return mesh;
        }

        private static void AddVertex(Vector3 vertex, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uv)
        {
            vertex.Normalize();
            vertices.Add(vertex);
            normals.Add(vertex);
            uv.Add(new Vector2(MathF.Atan2(vertex.Z, vertex.X) / (2 * MathF.PI) + 0.5f, MathF.Asin(vertex.Y) / MathF.PI + 0.5f));
        }

        private static int GetMiddlePoint(int p1, int p2, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uv)
        {
            // Key for the edge (smaller index first)
            int smallerIndex = Math.Min(p1, p2);
            int greaterIndex = Math.Max(p1, p2);
            long key = (long)smallerIndex << 32 | greaterIndex;

            if (middlePointIndexCache.TryGetValue(key, out int value))
            {
                return value;
            }

            Vector3 point1 = vertices[p1];
            Vector3 point2 = vertices[p2];
            Vector3 middle = (point1 + point2) / 2.0f;

            int newIndex = vertices.Count;
            AddVertex(middle, vertices, normals, uv);
            middlePointIndexCache[key] = newIndex;
            return newIndex;
        }

        struct TriangleIndices
        {
            public int v1, v2, v3;

            public TriangleIndices(int v1, int v2, int v3)
            {
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;
            }
        }

        private static Dictionary<long, int> middlePointIndexCache = new Dictionary<long, int>();
    }
}
