using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public abstract class CollisionShape
    {
        public abstract void getAabb(VIntTransform t, out VInt3 aabbMin, out VInt3 aabbMax);

        public abstract BroadphaseNativeType getShapeType();

        public abstract void setMargin(VFixedPoint margin);

        public abstract VFixedPoint getMargin();

        public abstract VInt3 support(VInt3 dir);

    }
}