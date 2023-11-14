using System;
using OpenTK.Mathematics;

namespace SpaceEngine.Entity_Component_System.Components
{
    internal class GlowEffect : Component
    {

        public Vector3 color { get; set; }
        public float glowRadius { get; set; }
        public GlowEffect(Vector3 color, float glowRadius)
        {
            this.color = color;
            this.glowRadius = glowRadius;

            EntityManager.postGeometrySystem.addMember(this);
        }
    }
}
