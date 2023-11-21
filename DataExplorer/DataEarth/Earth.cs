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
using SpaceEngine;
using System.Collections;
using DataExplorer.Modelling;

namespace DataExplorer.DataEarth
{
    class Country
    {
        public string name;
        public string code;
        public List<Vector3i> textureDataGroups = new List<Vector3i>();  //X = textureBuffer. Y=texture. Z=count
        public int id;
    }

    internal class Earth
    {
        public int[] countryHighlights = new int[255];



        public FrameBuffer dataFrameBuffer;
        public List<Country> countries = new List<Country>();
        private ShaderProgram countryPolygonShader = new ShaderProgram("Simple_Vertex", "Country_Polygon_Fragment");
        private ShaderProgram countryBorderShader = new ShaderProgram("Simple_Vertex", "Country_Border_Fragment");
        public Modelling.Texture countriesDataTexture;
        private string countriesDataTextureFilename = "countriesDataTexture";
        public Earth()
        {
            FrameBufferSettings fboSettings = new FrameBufferSettings(new Vector2i(4096/1, 2048/1));
            DrawBufferSettings drawBufferSettings = new DrawBufferSettings(FramebufferAttachment.ColorAttachment0);
            drawBufferSettings.formatInternal = PixelInternalFormat.Rgba8;
            drawBufferSettings.pixelType = PixelType.UnsignedByte;
            fboSettings.drawBuffers.Add(drawBufferSettings);
            dataFrameBuffer = new FrameBuffer(fboSettings);


            countryPolygonShader.bind();
            countryPolygonShader.loadUniformInt("borderPoints", 0);

            countryBorderShader.bind();
            countryBorderShader.loadUniformInt("borderPoints", 0);




            //reloadCountriesDataTexture();
            dataFrameBuffer.cleanUp();
            TextureSettings settings = new TextureSettings();
            settings.minFilter = OpenTK.Graphics.OpenGL4.TextureMinFilter.Nearest;
            settings.magFilter = OpenTK.Graphics.OpenGL4.TextureMagFilter.Nearest;
            countriesDataTexture = TextureMaster.loadTexture(countriesDataTextureFilename, settings);


        }
        private void reloadCountriesDataTexture()
        {
            loadBoarderModels();
            dataFrameBuffer.bind();
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            renderBorders();
            renderCountries();

            foreach (Country country in countries)
            {
                foreach (Vector3i borderGroup in country.textureDataGroups)
                {
                    GL.DeleteBuffer(borderGroup.X);
                    GL.DeleteTexture(borderGroup.Y);
                }
            }
            dataFrameBuffer.saveRenderAttachmentToFile(0, "Textures\\" + countriesDataTextureFilename);
        }

        private void renderBorders()
        {
            dataFrameBuffer.bind();
            countryBorderShader.bind();


            GL.ColorMask(0, false, true, false, false);
            foreach (Country country in countries)
            {
                countryBorderShader.loadUniformInt("countryID", country.id);
                foreach (Vector3i borderGroup in country.textureDataGroups)
                {
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.TextureBuffer, borderGroup.Y);
                    countryBorderShader.loadUniformInt("numBorderPoints", borderGroup.Z);

                    renderQuad();
                }
            }
            GL.ColorMask(0, true, true, true, true);
        }
        private void renderCountries()
        {
            dataFrameBuffer.bind();
            countryPolygonShader.bind();

            GL.ColorMask(0, true, false, false, false);
            foreach (Country country in countries)
            {
                countryPolygonShader.loadUniformInt("countryID", country.id);
                foreach (Vector3i borderGroup in country.textureDataGroups)
                {
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.TextureBuffer, borderGroup.Y);
                    countryPolygonShader.loadUniformInt("numBorderPoints", borderGroup.Z);

                    renderQuad();
                }
            }
            GL.ColorMask(0, true, true, true, true);
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
            string[] countriesStrings = StringSplitBetween(allText, '{', '}');
            int id = 1;
            foreach (string countryString in countriesStrings)
            {
                Country country = new Country();
                country.id = id;
                string countryData = countryString.Split("\"type\"")[2];
                bool multipolygon = false;
                foreach (string data in countryData.Split(","))
                {
                    string prefix = data.Split(":")[0].Trim().Replace("\"", "").Replace("}", "");

                    if (data.Replace(":", "").Replace("\"", "").Replace("}", "").Trim() == "MultiPolygon")
                    {
                        multipolygon = true;
                    }
                    {
                        if (prefix == "iso3")
                        {
                            country.code = data.Split(":")[1].Trim().Replace("\"", "");
                        }
                        if (prefix == "name")
                        {
                            country.name = data.Split(":")[1].Trim().Replace("\"", "");
                        }
                    }
                }
                string coordData = getStringBetween(countryString, '[', ']');
                if (multipolygon)
                {
                    string[] polygons = StringSplitBetween(coordData, '[', ']');
                    foreach (string polygon in polygons)
                    {
                        string[] polygonGroups = StringSplitBetween(polygon, '[', ']');
                        foreach (string polygonGroup in polygonGroups)
                        {
                            List<Vector2> polygonList = new List<Vector2>();
                            string[] coords = StringSplitBetween(polygonGroup, '[', ']');
                            foreach (string coord in coords)
                            {
                                string[] longLat = coord.Replace(" ", "").Split(",");
                                float latitude = (float)Double.Parse(longLat[1].Replace(".", ","));
                                float longitude = (float)Double.Parse(longLat[0].Replace(".", ","));
                                polygonList.Add(new Vector2((longitude / 180f + 1) / 2f, (latitude / 90f + 1) / 2f));
                            }
                            country.textureDataGroups.Add(loadTextureDataFromCoordList(polygonList));
                        }
                    }
                }
                else
                {
                    string[] polygonGroups = StringSplitBetween(coordData, '[', ']');
                    foreach (string polygonGroup in polygonGroups)
                    {
                        List<Vector2> polygon = new List<Vector2>();
                        string[] coords = StringSplitBetween(polygonGroup, '[', ']');
                        foreach (string coord in coords)
                        {
                            string[] longLat = coord.Replace(" ", "").Split(",");
                            float latitude = (float)Double.Parse(longLat[1].Replace(".", ","));
                            float longitude = (float)Double.Parse(longLat[0].Replace(".", ","));
                            polygon.Add(new Vector2((longitude / 180f + 1) / 2f, (latitude / 90f + 1) / 2f));
                        }
                        country.textureDataGroups.Add(loadTextureDataFromCoordList(polygon));
                    }
                }
                countries.Add(country);
                id++;
            }
            // Hack to fix country within country
            Country italy = countries.Find(x => x.code.Contains("ITA"));
            Country southAfrica = countries.Find(x => x.code.Contains("ZAF"));
            countries.Remove(italy);
            countries.Remove(southAfrica);
            countries.Insert(0, southAfrica);
            countries.Insert(0, italy);
        }

        private Vector3i loadTextureDataFromCoordList(List<Vector2> coordList)
        {
            float[] data = new float[coordList.Count * 2];
            for (int i = 0; i < coordList.Count; i++)
            {
                data[i * 2 + 0] = coordList[i].X;
                data[i * 2 + 1] = coordList[i].Y;
            }

            int textureBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.TextureBuffer, textureBuffer);
            GL.BufferData(BufferTarget.TextureBuffer, sizeof(float) * data.Length, data, BufferUsageHint.StaticDraw);

            int texture = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureBuffer, texture);
            GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.Rg32f, textureBuffer);

            return new Vector3i(textureBuffer, texture, coordList.Count);
        }

        private string getStringBetween(string input, char from, char to)
        {
            string result = "";
            int startIndex = -1;
            int endIndex = input.Length;

            for (int i = 0; i<input.Length; i++){
                if (input[i] == from)
                {
                    startIndex = i;
                    break;
                }
            }

            for (int i = input.Length-1; i >= 0; i--)
            {
                if (input[i] == to)
                {
                    endIndex = i;
                    break;
                }
            }
            input = input.Remove(endIndex, input.Length - endIndex);
            input = input.Remove(0, startIndex+1);

            return input;
        }
        private string[] StringSplitBetween(string input, char from, char to)
        {
            List<String> currentSplit = new List<String>();

            int depth = 0;
            StringBuilder stringBuilder = new StringBuilder(capacity: 10000);
            for (int i = 0; i <input.Length; i++)
            {
                if (input[i] == from)
                {
                    depth++;
                    if (depth == 1)
                    {
                        continue;
                    }
                }
                else if (input[i] == to)
                {
                    depth--;
                    if (depth == 0)
                    {
                        currentSplit.Add(stringBuilder.ToString());
                        stringBuilder.Clear();
                        continue;
                    }
                } 

                if (depth != 0)
                {
                    stringBuilder.Append(input[i]);
                }
            }
            return currentSplit.ToArray();
        }
    }
}
