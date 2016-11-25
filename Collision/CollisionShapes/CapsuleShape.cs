using System;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class CapsuleShape: ConvexInternalShape
    {
        protected int upAxis;

        public CapsuleShape(VFixedPoint radius, VFixedPoint height)
        {
            upAxis = 1;
            implicitShapeDimensions = new VInt3(radius, height * VFixedPoint.Half, radius);
        }

        public override VInt3 localGetSupportingVertexWithoutMargin(VInt3 vec0)
        {
            VInt3 supVec = VInt3.zero;
            VFixedPoint maxDot = VFixedPoint.MinValue;

            VInt3 vec = vec0;
            VFixedPoint lenSqr = vec.sqrMagnitude;
            if (lenSqr < Globals.EPS) {
                vec = VInt3.right;
            }
            else {
                vec = vec.Normalize();
            }

            VFixedPoint radius = getRadius();
            VInt3 pos = VInt3.zero;

            pos[getUpAxis()] = getHalfHeight();
            VInt3 tmp1 = vec * localScaling;
            tmp1 *= radius;
            VInt3 tmp2 = vec * getMargin();
            VInt3 vtx = pos + tmp1 - tmp2;
            VFixedPoint newDot = VInt3.Dot(vec, vtx);
            if (newDot > maxDot) {
                maxDot = newDot;
                supVec = vtx;
            }

            pos[getUpAxis()] = -getHalfHeight();
            tmp1 = vec * localScaling;
            tmp1 *= radius;
            tmp2 = vec * getMargin();
            vtx = pos + tmp1 - tmp2;
            newDot = VInt3.Dot(vec, vtx);
            if (newDot > maxDot) {
                maxDot = newDot;
                supVec = vtx;
            }

            return supVec;
        }

        public override void batchedUnitVectorGetSupportingVertexWithoutMargin(VInt3[] vectors, out VInt3[] supportVerticesOut) {
            throw new NotImplementedException();
        }

        public override void calculateLocalInertia(VFixedPoint mass, out VInt3 inertia) {
            // as an approximation, take the inertia of the box that bounds the spheres

            VIntTransform ident = VIntTransform.Identity;

            VFixedPoint radius = getRadius();

            VInt3 halfExtents = new VInt3(radius, radius, radius);
            halfExtents[getUpAxis()] = radius + getHalfHeight();

            VFixedPoint margin = Globals.CONVEX_DISTANCE_MARGIN;

            VFixedPoint lx = VFixedPoint.Two * (halfExtents.x + margin);
            VFixedPoint ly = VFixedPoint.Two * (halfExtents.y + margin);
            VFixedPoint lz = VFixedPoint.Two * (halfExtents.z + margin);
            VFixedPoint x2 = lx * lx;
            VFixedPoint y2 = ly * ly;
            VFixedPoint z2 = lz * lz;
            VFixedPoint scaledmass = mass / VFixedPoint.Create(12);

            inertia.x = scaledmass * (y2 + z2);
            inertia.y = scaledmass * (x2 + z2);
            inertia.z = scaledmass * (x2 + y2);
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

        public int getUpAxis() {
            return upAxis;
        }

        public VFixedPoint getRadius() {
            int radiusAxis = (upAxis + 2) % 3;
            return implicitShapeDimensions[radiusAxis];
        }

        public VFixedPoint getHalfHeight() {
            return implicitShapeDimensions[upAxis];
        }


    }
}