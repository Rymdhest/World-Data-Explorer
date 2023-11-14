using OpenTK.Mathematics;
namespace SpaceEngine.Entity_Component_System.Components
{
    internal class Momentum : Component
    {
        public Vector3 velocity;
        private bool hasMoved = false;
        public Momentum() :this (new Vector3(0))
        {
        }
        public Momentum(Vector3 velocity)
        {
            this.velocity = velocity;
        }

        public override void update(float delta)
        {
            //float weight = 0.1f;
            if (velocity.Length < 10.0f*delta)
            {
                velocity *= 0f;
                hasMoved = false;
            }
            else
            {
                //Console.WriteLine("Ball is moving at " + velocity.Length +" velocity");
                hasMoved = true;
                velocity = velocity / (1f + delta*0.05f); //friction
                owner.getComponent<Transformation>().position += velocity * delta;
            }

        }
        public bool HasMoved()
        {
            return hasMoved;
        }
    }
}
