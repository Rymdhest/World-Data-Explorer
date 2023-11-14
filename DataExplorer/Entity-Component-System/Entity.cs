
using SpaceEngine.Entity_Component_System.Components;

namespace SpaceEngine.Entity_Component_System
{
    internal class Entity
    {
        Dictionary<Type, Component> components;

        public Entity()
        {
            components= new Dictionary<Type, Component>();
            lock (EntityManager.threadLock)
            {
                EntityManager.entities.Add(this);
            }
        }

        public T getComponent<T>() where T : Component
        {
            Type type = typeof(T);
            if (components.TryGetValue(type, out Component component))
            {
                return (T)component;
            }
            else return null;
        }

        public void addComponent<T>(T component) where T : Component
        {
            component.owner = this;
            components[typeof(T)] = component;
            component.initialize();
        }
        public void cleanUp()
        {
            foreach (Component component in components.Values)
            {
                component.cleanUp();
            }
            components.Clear();
            EntityManager.entities.Remove(this);
        }
        public Boolean hasComponent<T>() where T : Component
        {
            return components.ContainsKey(typeof(T));
        }
        public void removeComponent<T>() where T : Component
        {
            Type type = typeof(T);
            if (components.TryGetValue(type, out Component component))
            {
                component.cleanUp();
                components.Remove(typeof(T));
            }
        }
        public void updateComponents(float delta)
        {
            foreach (Component component in components.Values)
            {
                component.update(delta);
            }
        }
    }
}
