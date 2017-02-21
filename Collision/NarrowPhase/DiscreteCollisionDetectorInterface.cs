using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public abstract class DiscreteCollisionDetectorInterface
    {
        public abstract class Result
        {
            public abstract void addContactPoint(VInt3 normalOnBInWorld, VFixedPoint depth);

            public bool hasContact
            {
                protected set; get;
            }

            public abstract void PreStep(VFixedPoint dt);

            public abstract void ApplyImpulse(VFixedPoint dt);
        }

        public abstract void getClosestPoints(ClosestPointInput input, Result output);
    }

    public class ClosestPointInput
    {
        public VIntTransform transformA = VIntTransform.Identity;
        public VIntTransform transformB = VIntTransform.Identity;
        public VFixedPoint maximumDistanceSquared;

        public ClosestPointInput()
        {
            init();
        }

        public void init()
        {
            maximumDistanceSquared = VFixedPoint.MaxValue;
        }
    }
}
