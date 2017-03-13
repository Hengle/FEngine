using System;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class BoxShape: CollisionShape
    {
        VInt3 halfExtent;
        VFixedPoint margin;

        public BoxShape(VInt3 boxHalfExtends)
        {
            halfExtent = boxHalfExtends;
        }

        public VInt3 getHalfExtent()
        {
            return halfExtent;
        }

        public override BroadphaseNativeType getShapeType()
        {
            return BroadphaseNativeType.BOX_SHAPE_PROXYTYPE;
        }

        public override void setMargin(VFixedPoint margin)
        {
            this.margin = margin;
        }

        public override VFixedPoint getMargin()
        {
            return margin;
        }

        public override void getAabb(VIntTransform trans, out VInt3 aabbMin, out VInt3 aabbMax)
        {
            AabbUtils.transformAabb(getHalfExtent(), getMargin(), trans, out aabbMin, out aabbMax);
        }
    }
}
