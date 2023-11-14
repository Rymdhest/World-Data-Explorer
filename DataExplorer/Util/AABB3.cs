using OpenTK.Mathematics;

namespace SpaceEngine.Util
{
    internal class AABB3
    {
        public Vector3 min;
        public Vector3 max;

        public AABB3(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
        }
    }
}
