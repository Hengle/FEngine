using System;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class SphereShape: ConvexInternalShape
    {
        public SphereShape(VFixedPoint radius) {
            implicitShapeDimensions.x = radius;
            collisionMargin = radius;
        }

        public override VInt3 localGetSupportingVertexWithoutMargin(VInt3 vec)
        {
            return VInt3.zero;
        }

        public override void batchedUnitVectorGetSupportingVertexWithoutMargin(VInt3[] vectors, out VInt3[] supportVerticesOut)
        {
            supportVerticesOut = new VInt3[vectors.Length];
            for (int i = 0; i < supportVerticesOut.Length; i++) {
                supportVerticesOut[i] = VInt3.zero;
            }
        }

        public override void getAabb(VIntTransform t, out VInt3 aabbMin, out VInt3 aabbMax)
        {
            VInt3 center = t.position;
            VInt3 extent = new VInt3(getMargin(), getMargin(), getMargin());
            aabbMin = center - extent;
            aabbMax = center + extent;
        }

        public override BroadphaseNativeType getShapeType() {
            return BroadphaseNativeType.SPHERE_SHAPE_PROXYTYPE;
        }

        public override void calculateLocalInertia(VFixedPoint mass, out VInt3 inertia)
        {
            VFixedPoint elem = VFixedPoint.Create(4) / VFixedPoint.Create(10) * mass * getMargin() * getMargin();
            inertia = new VInt3(elem, elem, elem);
        }

        public VFixedPoint getRadius() {
            return implicitShapeDimensions.x * localScaling.x;
        }

        public override void setMargin(VFixedPoint margin) {

        }

        public override VFixedPoint getMargin() {
            // to improve gjk behaviour, use radius+margin as the full margin, so never get into the penetration case
            // this means, non-uniform scaling is not supported anymore
            return getRadius();
        }


    }
}