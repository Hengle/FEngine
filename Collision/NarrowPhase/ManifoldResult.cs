using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class ManifoldResult : DiscreteCollisionDetectorInterface.Result
    {
        // we need this for compounds
        public CollisionObject body0;
        public CollisionObject body1;
        public VInt3 normalWorldOnB;
        public VFixedPoint depth;
        
        public ManifoldResult()
        {

        }

        public void init(CollisionObject body0, CollisionObject body1)
        {
            this.body0 = body0;
            this.body1 = body1;
            hasContact = false;
        }

        public override void addContactPoint(VInt3 normalOnBInWorld, VFixedPoint depth)
        {
            normalWorldOnB = normalOnBInWorld;
            this.depth = depth;
            hasContact = true;
        }
    }
}
