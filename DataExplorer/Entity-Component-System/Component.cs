using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEngine.Entity_Component_System
{
    internal abstract class Component
    {
        public Entity owner { get; set; }
        private List<ComponentSystem> subscribedSystem = new List<ComponentSystem>();

        public virtual void initialize()   { }
        public virtual void cleanUp()
        {
            for (int i = subscribedSystem.Count - 1 ; i >= 0; i--)
            {
                subscribedSystem[i].removeMember(this);
            }
            subscribedSystem.Clear();
        }
        public void addSubscribedSystem(ComponentSystem system)
        {
            subscribedSystem.Add(system);
        }
        public void removeSubscribedSystem(ComponentSystem system)
        {
            subscribedSystem.Remove(system);
        }
        public virtual void update(float delta) { }
    }
}
