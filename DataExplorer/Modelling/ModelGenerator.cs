using OpenTK.Mathematics;
using SpaceEngine.RenderEngine;

namespace SpaceEngine.Modelling
{
    internal class ModelGenerator
    {
        public static glModel unitSphere = glLoader.loadToVAO(MeshGenerator.CreateIcosphere(2));

    }
}
