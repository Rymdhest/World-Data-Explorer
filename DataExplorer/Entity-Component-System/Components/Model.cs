

using SpaceEngine.Entity_Component_System;
using SpaceEngine.Entity_Component_System.Components;
using SpaceEngine.Modelling;

namespace SpaceEngine.RenderEngine
{
    internal class Model : Component
    {

        private glModel model;
        private MasterRenderer.Pipeline pipeline;
        private bool cleanGLModel;

        public Model(glModel model, MasterRenderer.Pipeline pipeline, bool cleanGLModel = true)
        {
            this.model = model;
            this.pipeline = pipeline;
            this.cleanGLModel = cleanGLModel;


            this.cleanGLModel = cleanGLModel;
        }

        public override void initialize()
        {
            if (pipeline == MasterRenderer.Pipeline.FLAT_SHADING)
            {
                EntityManager.flatShadingSystem.addMember(this);
            }
            else if (pipeline == MasterRenderer.Pipeline.SMOOTH_SHADING)
            {
                EntityManager.smoothShadingSystem.addMember(this);
            }
        }

        public glModel getModel()
        {
            return model;
        }

        public MasterRenderer.Pipeline getPipeline()
        {
            return pipeline;
        }



        public override void cleanUp()
        {
            base.cleanUp();
            if (cleanGLModel)
            {
                model.cleanUp();
            }
            
        }
    }
}
