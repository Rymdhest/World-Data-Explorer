using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;

namespace DataExplorer.Modelling
{
    public class TextureSettings
    {
        public TextureMinFilter minFilter = TextureMinFilter.Linear;
        public TextureMagFilter magFilter = TextureMagFilter.Linear;
    }
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
        public byte[] getGPUMemoryData()
        {
            GL.ReadBuffer(ReadBufferMode.Back);
            byte[] imageData = new byte[resolution.X * resolution.Y * 4];
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.UnsignedByte, imageData);

            return imageData;
        }
        public byte[] getGPUMemoryAtUV(Vector2 uv)
        {

            byte[] imageData = getGPUMemoryData();
            int x = (int)(uv.X * (resolution.X-1));
            int y = (int)(uv.Y * (resolution.Y-1));
            int pixelIndex = (y * resolution.X + x)*4;
            byte red = imageData[pixelIndex];
            byte green = imageData[pixelIndex + 1];
            byte blue = imageData[pixelIndex + 2];
            byte alpha = imageData[pixelIndex + 3];

            return new byte[] { red, green, blue, alpha};
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

        public static Texture loadTexture(string fileName)
        {
            return TextureMaster.loadTexture(fileName, new TextureSettings());
        }
        public static Texture loadTexture(string fileName, TextureSettings settings)
        {
            int textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            StbImage.stbi_set_flip_vertically_on_load(1);

            ImageResult image = ImageResult.FromStream(File.OpenRead("Textures\\"+fileName+".png"), ColorComponents.RedGreenBlueAlpha);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)settings.minFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)settings.magFilter);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            return new Texture(textureID, new Vector2i(image.Width, image.Height), fileName);
        }
    }
}
