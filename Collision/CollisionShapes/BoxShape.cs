using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class BoxShape: PolyhedralConvexShape
    {
        public BoxShape(VInt3 boxHalfExtends)
        {
            VInt3 margin = new VInt3(getMargin(), getMargin(), getMargin());
            implicitShapeDimensions = boxHalfExtends * localScaling;
            implicitShapeDimensions -= margin;
        }

        public VInt3 getHalfExtentsWithMargin()
        {
            VInt3 halfExtends = getHalfExtentsWithoutMargin();
            VInt3 margin = new VInt3(getMargin(), getMargin(), getMargin());
            return halfExtends + margin;
        }

        public VInt3 getHalfExtentsWithoutMargin()
        {
            return implicitShapeDimensions;
        }

        public override BroadphaseNativeType getShapeType()
        {
            return BroadphaseNativeType.BOX_SHAPE_PROXYTYPE;
        }

        public override VInt3 localGetSupportingVertex(VInt3 vec)
        {
            VInt3 halfExtents = getHalfExtentsWithoutMargin();

            VFixedPoint margin = getMargin();
            halfExtents.x += margin;
            halfExtents.y += margin;
            halfExtents.z += margin;

		    VInt3 result = new VInt3(
                vec.x >= VFixedPoint.Zero ? halfExtents.x : -halfExtents.x,
                vec.y >= VFixedPoint.Zero ? halfExtents.y : -halfExtents.y,
                vec.z >= VFixedPoint.Zero ? halfExtents.z : -halfExtents.z);
            return result;
        }

        public override void batchedUnitVectorGetSupportingVertexWithoutMargin(VInt3[] vectors, out VInt3[] supportVerticesOut)
        {
            int numVectors = vectors.Length;
            supportVerticesOut = new VInt3[numVectors];
            VInt3 halfExtents = getHalfExtentsWithoutMargin();

		    for (int i = 0; i<numVectors; i++) 
            {
			    VInt3 vec = vectors[i];
                supportVerticesOut[i] = new VInt3(vec.x >= VFixedPoint.Zero ? halfExtents.x : -halfExtents.x,
                vec.y >= VFixedPoint.Zero ? halfExtents.y : -halfExtents.y,
                vec.z >= VFixedPoint.Zero ? halfExtents.z : -halfExtents.z);
		    }
        }

        public override void setMargin(VFixedPoint margin)
        {
            VInt3 oldMargin = new VInt3(getMargin(), getMargin(), getMargin());
            VInt3 implicitShapeDimensionsWithMargin = implicitShapeDimensions + oldMargin;

		    base.setMargin(margin);
		    VInt3 newMargin = new VInt3(getMargin(), getMargin(), getMargin());
		    implicitShapeDimensions = implicitShapeDimensionsWithMargin -  newMargin;
        }

        public override void setLocalScaling(VInt3 scaling)
        {
            VInt3 oldMargin = new VInt3(getMargin(), getMargin(), getMargin());
		    VInt3 implicitShapeDimensionsWithMargin = implicitShapeDimensions + oldMargin;
		    VInt3 unScaledImplicitShapeDimensionsWithMargin = implicitShapeDimensionsWithMargin / localScaling;

		    base.setLocalScaling(scaling);

		    implicitShapeDimensions = unScaledImplicitShapeDimensionsWithMargin * localScaling;
		    implicitShapeDimensions -= oldMargin;
        }

        public override void getAabb(VIntTransform trans, out VInt3 aabbMin, out VInt3 aabbMax)
        {
            AabbUtils.transformAabb(getHalfExtentsWithoutMargin(), getMargin(), trans, out aabbMin, out aabbMax);
        }

        public override void calculateLocalInertia(VFixedPoint mass, out VInt3 inertia)
        {
            VInt3 halfExtents = getHalfExtentsWithMargin();
            VFixedPoint lx = VFixedPoint.Two * halfExtents.x;
            VFixedPoint ly = VFixedPoint.Two * halfExtents.y;
            VFixedPoint lz = VFixedPoint.Two * halfExtents.z;

            inertia = new VInt3(ly * ly + lz * lz,
                    lx * lx + lz * lz,
                    lx * lx + ly * ly) * mass / VFixedPoint.Create(12);
        }

        public override int getNumVertices()
        {
            return 8;
        }

        public override VInt3 getVertex(int i)
        {
            VInt3 halfExtents = getHalfExtentsWithoutMargin();

		    return new VInt3(halfExtents.x* (1 - (i & 1)) - halfExtents.x* (i & 1),
				halfExtents.y* (1 - ((i & 2) >> 1)) - halfExtents.y* ((i & 2) >> 1),
				halfExtents.z* (1 - ((i & 4) >> 2)) - halfExtents.z* ((i & 4) >> 2));
        }
    }
}
