
using SpaceEngine.Modelling;
using SpaceEngine.RenderEngine;
using System.Diagnostics.Metrics;

namespace SpaceEngine.Entity_Component_System.Systems
{
    internal class ModelSystem : ComponentSystem
    {
        private Dictionary<glModel, List<Entity>> models = new Dictionary<glModel, List<Entity>>();

        public override void addMember(Component member)
        {
            glModel glModel = member.owner.getComponent<Model>().getModel();
            if (models.ContainsKey(glModel)) {
                models[glModel].Add(member.owner);
            } else
            {
                models.Add(glModel, new List<Entity>());
                models[glModel].Add(member.owner);
            }
            member.addSubscribedSystem(this);
        }

        public override void removeMember(Component member)
        {
            Model model = (Model)member;
            glModel glmodel = model.owner.getComponent<Model>().getModel();

            models[glmodel].Remove(member.owner);

            if (models[glmodel].Count == 0)
            {
                models.Remove(glmodel);
            }
            member.removeSubscribedSystem(this);
        }

        public Dictionary<glModel, List<Entity>> getModels()
        {
            return models;
        }
    }
}
