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

            VInt3 halfExtents = VInt3.zero;
            halfExtents[upAxis] = getHalfHeight();

            VInt3[] basis = t.getTransposeBasis();

            VInt3 center = t.position;
            VInt3 extent = new VInt3();

            extent.x = VInt3.Dot(halfExtents, basis[0].Abs());
            extent.y = VInt3.Dot(halfExtents, basis[1].Abs());
            extent.z = VInt3.Dot(halfExtents, basis[2].Abs());

            aabbMin = center - extent - VInt3.one * getRadius();
            aabbMax = center + extent + VInt3.one * getRadius();
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

        public override VInt3 support(VInt3 dir)
        {
            if(dir[upAxis] > VFixedPoint.Zero)
            {
                return getUpAxis() * halfHeight + dir * radius;
            } 
            else
            {
                return -getUpAxis() * halfHeight + dir * radius;
            }
        }
    }

    public class LineShape : CollisionShape
    {
        protected VInt3 p0, p1; 

        public LineShape(CapsuleShape capsule)
        {
            p0 = capsule.getUpAxis() * -capsule.getHalfHeight();
            p1 = capsule.getUpAxis() * capsule.getHalfHeight();
        }

        public override void getAabb(VIntTransform t, out VInt3 aabbMin, out VInt3 aabbMax)
        {
            VInt3 halfExtents = p0.Abs(); 
            VInt3[] basis = t.getTransposeBasis();

            VInt3 center = t.position;
            VInt3 extent = new VInt3();

            extent.x = VInt3.Dot(halfExtents, basis[0].Abs());
            extent.y = VInt3.Dot(halfExtents, basis[1].Abs());
            extent.z = VInt3.Dot(halfExtents, basis[2].Abs());

            aabbMin = center - extent;
            aabbMax = center + extent;
        }

        public override VFixedPoint getMargin()
        {
            return VFixedPoint.Zero; 
        }

        public override BroadphaseNativeType getShapeType()
        {
            return BroadphaseNativeType.CAPSULE_SHAPE_PROXYTYPE;
        }

        public override void setMargin(VFixedPoint margin)
        {
            
        }

        public override VInt3 support(VInt3 dir)
        {
            return VInt3.Dot(dir, p0) > VInt3.Dot(dir, p1) ? p0 : p1;
        }
    }

}