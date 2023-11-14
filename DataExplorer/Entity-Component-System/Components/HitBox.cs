using SpaceEngine.Util;
using OpenTK.Mathematics;

namespace SpaceEngine.Entity_Component_System.Components
{
    internal class HitBox : Component
    {
        public AABB3 hitBox;


        public HitBox(Vector3 min, Vector3 max) : this(new AABB3(min, max)) { }
        public HitBox(AABB3 hitBox) 
        {
            this.hitBox = hitBox;
        }

    }
}
