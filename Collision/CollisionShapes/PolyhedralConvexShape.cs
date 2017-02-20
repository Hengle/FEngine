using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public abstract class PolyhedralConvexShape: ConvexInternalShape
    {
        private static VInt3[] _directions = new VInt3[]
        {
            VInt3.right,
            VInt3.up,
            VInt3.forward,
            -VInt3.right,
            -VInt3.up,
            -VInt3.forward
        };

        private static VInt3[] _supporting = new VInt3[]
        {
            VInt3.zero,
            VInt3.zero,
            VInt3.zero,
            VInt3.zero,
            VInt3.zero,
            VInt3.zero
        };

        protected VInt3 localAabbMin = VInt3.one;
        protected VInt3 localAabbMax = -VInt3.one;
        protected bool isLocalAabbValid = false;

        public override VInt3 localGetSupportingVertexWithoutMargin(VInt3 vec0)
        {
            VInt3 supVec = VInt3.zero;

            VFixedPoint maxDot = VFixedPoint.MinValue;

            VInt3 vec = vec0;
            VFixedPoint lenSqr = vec.sqrMagnitude;
            if (lenSqr < Globals.EPS)
            {
                vec = VInt3.right;
            }
            else
            {
                vec = vec.Normalize();
            }

            VInt3 vtx;

            for (int i = 0; i < getNumVertices(); i++)
            {
                vtx = getVertex(i);
                VFixedPoint newDot = VInt3.Dot(vec, vtx);
                if (newDot > maxDot)
                {
                    maxDot = newDot;
                    supVec = vtx;
                }
            }

            return supVec;
        }

        public override void getAabb(VIntTransform trans, out VInt3 aabbMin, out VInt3 aabbMax)
        {
            AabbUtils.transformAabb(localAabbMin, localAabbMax, getMargin(), trans, out aabbMin, out aabbMax);
        }

        protected void _PolyhedralConvexShape_getAabb(VIntTransform trans, out VInt3 aabbMin, out VInt3 aabbMax)
        {
            AabbUtils.transformAabb(localAabbMin, localAabbMax, getMargin(), trans, out aabbMin, out aabbMax);
        }

        public void recalcLocalAabb() {
            isLocalAabbValid = true;

            for(int i = 0; i < _directions.Length; i++)
            {
                _supporting[i] = localGetSupportingVertexWithoutMargin(_directions[i]);
            }

            for (int i=0; i<3; i++) {
                localAabbMax[i] = _supporting[i][i] + collisionMargin;
                localAabbMin[i] = _supporting[i + 3][i] - collisionMargin;
            }
        }

        public override void setLocalScaling(VInt3 scaling) {
            base.setLocalScaling(scaling);
            recalcLocalAabb();
        }

        public abstract int getNumVertices();

        public abstract VInt3 getVertex(int i);
    }
}