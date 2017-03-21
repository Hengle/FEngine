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

        public override VInt3 support(VInt3 dir)
        {
            return new VInt3(dir.x > VFixedPoint.Zero ? VFixedPoint.One : -VFixedPoint.One,
                dir.y > VFixedPoint.Zero ? VFixedPoint.One : -VFixedPoint.One,
                dir.z > VFixedPoint.Zero ? VFixedPoint.One : -VFixedPoint.One
                ) * halfExtent;
        }
    }
}
