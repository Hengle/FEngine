using System;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class CapsuleShape: CollisionShape
    {
        protected int upAxis;
        protected VFixedPoint halfHeight;
        protected VFixedPoint radius;
        protected VFixedPoint margin;

        public CapsuleShape(VFixedPoint radius, VFixedPoint height)
        {
            upAxis = 1;
            this.halfHeight = height;
            this.radius = radius;
        }

        public override BroadphaseNativeType getShapeType() {
            return BroadphaseNativeType.CAPSULE_SHAPE_PROXYTYPE;
        }

        public override void getAabb(VIntTransform t, out VInt3 aabbMin, out VInt3 aabbMax) {

            VInt3 halfExtents = new VInt3(getRadius(), getRadius(), getRadius());
            halfExtents[upAxis] = getRadius() + getHalfHeight();

            halfExtents.x += getMargin();
            halfExtents.y += getMargin();
            halfExtents.z += getMargin();

            VInt3 center = t.position;
            VInt3 extent = new VInt3();

            extent.x = VInt3.Dot(halfExtents, new VInt3(t.right.x.Abs(), t.right.y.Abs(), t.right.z.Abs()));
            extent.y = VInt3.Dot(halfExtents, new VInt3(t.up.x.Abs(), t.up.y.Abs(), t.up.z.Abs()));
            extent.z = VInt3.Dot(halfExtents, new VInt3(t.forward.x.Abs(), t.forward.y.Abs(), t.forward.z.Abs()));

            aabbMin = center - extent;
            aabbMax = center + extent;
        }

        public VInt3 getUpAxis() {
            VInt3 upAxisVector = VInt3.zero;
            upAxisVector[upAxis] = VFixedPoint.One;
            return upAxisVector;
        }

        public VFixedPoint getRadius() {
            return radius; 
        }

        public VFixedPoint getHalfHeight() {
            return halfHeight; 
        }

        public override void setMargin(VFixedPoint margin)
        {
            this.margin = margin;
        }

        public override VFixedPoint getMargin()
        {
            return margin;
        }
    }
}