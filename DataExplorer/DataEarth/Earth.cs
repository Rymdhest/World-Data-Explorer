using SpaceEngine.RenderEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using SpaceEngine.Modelling;
using SpaceEngine.Shaders;
using SpaceEngine.Util;
using System.Drawing.Drawing2D;

namespace DataExplorer.DataEarth
{
    internal class Earth
    {
        public FrameBuffer dataFrameBuffer;
        public List<List<Vector2>> countries = new List<List<Vector2>>();
        private ShaderProgram lineShader = new ShaderProgram("Simple_Vertex", "Line_Fragment");
        public Earth()
        {
            FrameBufferSettings fboSettings = new FrameBufferSettings(new Vector2i(4096, 2048));
            DrawBufferSettings drawBufferSettings = new DrawBufferSettings(FramebufferAttachment.ColorAttachment0);
            drawBufferSettings.formatInternal = PixelInternalFormat.Rgba;
            fboSettings.drawBuffers.Add(drawBufferSettings);
            dataFrameBuffer = new FrameBuffer(fboSettings);

            loadBoarderModels();
            //renderBorders();
            renderCountries();
        }

        private void renderCountries()
        {
            dataFrameBuffer.bind();
            lineShader.bind();

            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            int id = 1;
            foreach (List<Vector2> country in countries)
            {
                lineShader.loadUniformInt("countryID", id);
                lineShader.loadUniformInt("numBorderPoints", country.Count);
                lineShader.loadUniformVector2fArray("borderPoints", country.ToArray());
                renderQuad();
                id++;
            }
        }

        private void renderQuad()
        {
            glModel quadModel = ScreenQuadRenderer.quadModel;
            GL.BindVertexArray(quadModel.getVAOID());
            GL.EnableVertexAttribArray(0);



            GL.Disable(EnableCap.DepthTest);
            GL.DepthMask(false);

            GL.Disable(EnableCap.Blend);

            GL.DrawElements(PrimitiveType.Triangles, quadModel.getVertexCount(), DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
        }
        /*
        private void renderBorders()
        {


            dataFrameBuffer.bind();
            lineShader.bind();
            GL.Disable(EnableCap.DepthTest);
            GL.DepthMask(false);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            foreach (glModel borderModel in borderModels)
            {
                GL.BindVertexArray(borderModel.getVAOID());
                GL.EnableVertexAttribArray(0);
                GL.DrawElements(PrimitiveType.LineLoop, borderModel.getVertexCount(), DrawElementsType.UnsignedInt, 0);
            }

        }
        */
        private void loadBoarderModels()
        {
            StreamReader sr = new StreamReader(System.IO.Path.GetFullPath("Data\\world-administrative-boundaries.json"));
            string allText = sr.ReadToEnd();
            string[] countryNames = allText.Split("{\"coordinates\":");
            int counter = 0;
            for (int i = 1; i < countryNames.Length; i++)
            {
                string countryName = countryNames[i];
                //string[] inner = bracket.Split("]");
                //Console.WriteLine("NEW COUNTRY\n");

                string[] borderGroups = countryName.Split("[[[");
                string countryData = borderGroups[borderGroups.Length - 1].Split("]]]")[1];
                Console.WriteLine(countryData);
                for (int j = 0; j < borderGroups.Length; j++)
                {
                    string group = borderGroups[j].Split("]]]")[0];
                    string[] coords = group.Trim().Replace("[", "").Replace("]", "").Split(",");

                    List<Vector2> country = new List<Vector2>();
                    for (int k = 0; k < coords.Length - 1; k += 2)
                    {

                        float latitude = (float)Double.Parse(coords[k + 1].Replace(".", ","));
                        float longitude = (float)Double.Parse(coords[k].Replace(".", ","));

                        country.Add(new Vector2(longitude/180f, latitude/90f));
                    }
                    countries.Add(country);
                }
            }
            Console.WriteLine("loaded " + counter + " border lines");
        }
    }
}
