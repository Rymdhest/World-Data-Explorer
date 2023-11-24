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
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;
using System.Diagnostics.Metrics;

namespace DataExplorer.DataEarth
{
    class Country
    {
        public string name;
        public string iso3;
        public string iso2;
        public List<Vector3i> textureDataGroups = new List<Vector3i>();  //X = textureBuffer. Y=texture. Z=count
        public int dataTextureID;



        override public string ToString()
        {
            return dataTextureID+" : "+ name+" : "+iso2+" : "+iso3;
        }
    }

    internal class Earth
    {
        public int[] countryHighlights = new int[256];



        public FrameBuffer dataFrameBuffer;
        public List<Country> countries = new List<Country>();
        private ShaderProgram countryPolygonShader = new ShaderProgram("Simple_Vertex", "Country_Polygon_Fragment");
        private ShaderProgram countryBorderShader = new ShaderProgram("Simple_Vertex", "Country_Border_Fragment");
        public Modelling.Texture countriesDataTexture;
        private string countriesDataTextureFilename = "countriesDataTexture";

        public Texture flagsTextureArray;

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

            string[] lines = File.ReadAllLines("Textures\\"+countriesDataTextureFilename+".txt");
            foreach (string line in lines)
            {
                Country country = new Country();
                string[] words = line.Split(" ");
                if (words.Length < 2) continue;
                country.dataTextureID = int.Parse(words[0]);
                country.iso3 = words[1];
                country.iso2 = words[2];
                country.name = words[3];

                countries.Add(country);

                //Console.WriteLine(country.ToString());
            }
            Console.WriteLine("Added " + countries.Count + " countries.");
            flagsTextureArray = loadFlagArrayTexture();
            //generateSquareFlagTextures();
        }

        private Texture loadFlagArrayTexture()
        {
            int textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DArray, textureID);

            StbImage.stbi_set_flip_vertically_on_load(1);
            string folderPath = "Textures\\Flags\\Squares\\";
            int width = 256;
            int height = 256; //240

            byte[] data = new byte[width * height*4*(countries.Count+1)];
            
            foreach (Country country in countries)
            {
                if (!File.Exists(folderPath+country.iso2+".png"))
                {
                    Console.WriteLine(country.ToString()+" - does not have a flag");
                    continue;
                }
                ImageResult image = ImageResult.FromStream(File.OpenRead(folderPath + country.iso2 + ".png"), ColorComponents.RedGreenBlueAlpha);
                for (int i = 0; i<width*height*4; i++)
                {
                    int offset = country.dataTextureID*width*height*4;
                    data[offset+i] = image.Data[i];
                }

            }
            //GL.TexImage3D(TextureTarget.Texture2DArray, 0, 0, 0, 0, width, height, countries.Count, PixelFormat.Rgba, PixelType.UnsignedByte, data);

            GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.Rgba, width, height, countries.Count+1, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            //GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);

            return new Texture(textureID, new Vector2i(width, height), "Flags Array Texture");



        }
        private void generateSquareFlagTextures()
        {
            string folderPath = "Textures\\Flags\\";
            string[] files = Directory.GetFiles(folderPath);


            int resolution = 256;
            FrameBufferSettings FBOsettings = new FrameBufferSettings(new Vector2i(resolution, resolution)); ;


            DrawBufferSettings drawBufferSettings = new DrawBufferSettings(FramebufferAttachment.ColorAttachment0);
            drawBufferSettings.formatInternal = PixelInternalFormat.Rgba8;
            drawBufferSettings.pixelType = PixelType.UnsignedByte;
            FBOsettings.drawBuffers.Add(drawBufferSettings);

            FrameBuffer flagFBO = new FrameBuffer(FBOsettings);
            flagFBO.bind();
            GL.ActiveTexture(TextureUnit.Texture0);
            ShaderProgram passShader = new ShaderProgram("Simple_Vertex", "Simple_Fragment");
            passShader.bind();
            foreach (string file in files)
            {
                Texture texture = TextureMaster.loadTexture("Flags\\"+file.Split("\\")[2].Split(".")[0]);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, texture.textureID);
                ScreenQuadRenderer.render(clearColor: true);
                flagFBO.saveRenderAttachmentToFile(0, "Textures\\Flags\\Squares\\"+ file.Split("\\")[2].Split(".")[0]);
            }
            passShader.cleanUp();
            flagFBO.cleanUp();

        }
        public void update(Vector3 cameraPosition)
        {
            if (WindowHandler.gameWindow.IsMouseButtonPressed(MouseButton.Left))
            {
                // Convert mouse coordinates to NDC
                float ndcX = (2.0f * WindowHandler.gameWindow.MousePosition.X) / WindowHandler.resolution.X - 1.0f;
                float ndcY = 1.0f - (2.0f * WindowHandler.gameWindow.MousePosition.Y) / WindowHandler.resolution.Y;


                // Create a ray in view space
                Vector4 rayClip = new Vector4(ndcX, ndcY, -1.0f, 1.0f);
                Matrix4 invProjMat = Matrix4.Invert(MasterRenderer.getProjectionMatrix());
                Vector4 rayEye = rayClip*invProjMat;
                rayEye = new Vector4(rayEye.X, rayEye.Y, -1.0f, 0.0f);

                // Convert ray to world space
                Matrix4 invViewMat = Matrix4.Invert(MasterRenderer.getViewMatrix());
                Vector4 rayWorld =  rayEye* invViewMat;
                Vector3 rayDir = Vector3.Normalize(new Vector3(rayWorld));

                Vector3 sphereCenter = new Vector3(0);
                Vector3 intersectionPoint;
                float sphereRadius = 200;
                if (MyMath.RaySphereIntersect(cameraPosition, rayDir, sphereCenter, sphereRadius, out intersectionPoint))
                {
                    // Calculate spherical coordinates
                    float phi = (float)Math.Asin((intersectionPoint.Y - sphereCenter.Y) / sphereRadius);
                    float theta = (float)Math.Atan2(intersectionPoint.X - sphereCenter.X, intersectionPoint.Z - sphereCenter.Z);

                    // Map spherical coordinates to UV coordinates
                    float u = (theta + MathF.PI) / (2.0f * MathF.PI);
                    float v = (phi + (float)Math.PI / 2.0f) / (float)Math.PI;

                    Vector2 uv = new Vector2(u, v);

                    int countryID = countriesDataTexture.getGPUMemoryAtUV(uv)[0];

                    if (countryID != 0)
                    {
                        Country clickedCountry = countries.Find(x => x.dataTextureID==countryID);
                        Console.WriteLine(clickedCountry.ToString());

                        for (int i = 0; i<countryHighlights.Length; i++) countryHighlights[i] = 0;
                        countryHighlights[countryID] = 1;
                    }
                }
            }
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
            saveDataTextureIDtoISO3Lookup();
            countries.Clear();
        }

        private void saveDataTextureIDtoISO3Lookup()
        {
            List<string> IDtoISO3 = new List<string>();

            for (int i = 0; i<countries.Count; i++)
            {
                IDtoISO3.Add(countries[i].dataTextureID+" "+ countries[i].iso3 + " " + countries[i].iso2 + " " + countries[i].name);
            }


            string[] lines = IDtoISO3.ToArray();
            string fullFilePath = "Textures\\" + countriesDataTextureFilename + ".txt";

            if (File.Exists(fullFilePath))
            {
                File.Delete(fullFilePath);
            }
            File.WriteAllLines(fullFilePath, lines);
        }

        private void renderBorders()
        {
            dataFrameBuffer.bind();
            countryBorderShader.bind();


            GL.ColorMask(0, false, true, false, false);
            foreach (Country country in countries)
            {
                countryBorderShader.loadUniformInt("countryID", country.dataTextureID);
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
                countryPolygonShader.loadUniformInt("countryID", country.dataTextureID);
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
                country.dataTextureID = id;
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
                            country.iso3 = data.Split(":")[1].Trim().Replace("\"", "");
                        }
                        if (prefix == "name")
                        {
                            country.name = data.Split(":")[1].Trim().Replace("\"", "");
                        }
                        if (prefix == "iso_3166_1_alpha_2_codes")
                        {
                            country.iso2 = data.Split(":")[1].Trim().Replace("\"", "");
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
                if (country.iso3 == "null") continue;
                if (country.iso2 == "null") continue;
                countries.Add(country);
                id++;
            }
            // Hack to fix country within country
            
            Country italy = countries.Find(x => x.iso3.Contains("ITA"));
            Country southAfrica = countries.Find(x => x.iso3.Contains("ZAF"));
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
