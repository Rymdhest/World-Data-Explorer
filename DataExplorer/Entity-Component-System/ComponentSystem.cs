
namespace SpaceEngine.Entity_Component_System
{
    internal class ComponentSystem
    {
        private List<Component> memberComponents = new List<Component>();

        public ComponentSystem()
        {

        }
        public virtual void addMember(Component member)
        {
            memberComponents.Add(member);
            member.addSubscribedSystem(this);
        }
        public virtual void removeMember(Component member)
        {
            memberComponents.Remove(member);
            member.removeSubscribedSystem(this);

            Console.WriteLine("removing member in base system");
        }

        public List<Component> getMembers()
        {
            return memberComponents;
        }
    }

}
