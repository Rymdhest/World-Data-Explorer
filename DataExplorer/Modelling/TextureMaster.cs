using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;

namespace DataExplorer.Modelling
{

    public class Texture
    {
        public int textureID;
        public string name;
        public Vector2i resolution;

        public Texture(int id, Vector2i resolution, string name)
        {
            this.textureID = id;
            this.resolution = resolution;
            this.name = name;
        }
    }
    internal class TextureMaster
    {
        //public static Texture earthAlbedo = loadTexture("color_june 21600x10800");
        //public static Texture earthTopography = loadTexture("topography_21600x10800");

        public static Texture earthAlbedo = loadTexture("color_june 2048x1024");
        public static Texture earthTopography = loadTexture("topography_2048x1024");

        public TextureMaster()
        {

        }

        private static Texture loadTexture(string fileName)
        {
            int textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            StbImage.stbi_set_flip_vertically_on_load(1);

            ImageResult image = ImageResult.FromStream(File.OpenRead("Textures\\"+fileName+".png"), ColorComponents.RedGreenBlueAlpha);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            return new Texture(textureID, new Vector2i(image.Width, image.Height), fileName);
        }
    }
}
