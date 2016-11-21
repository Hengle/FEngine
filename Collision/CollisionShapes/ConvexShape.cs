using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public abstract class ConvexShape: CollisionShape
    {
        public static readonly int MAX_PREFERRED_PENETRATION_DIRECTIONS = 10;

        public abstract VInt3 localGetSupportingVertex(VInt3 vec);

        public abstract VInt3 localGetSupportingVertexWithoutMargin(VInt3 vec);

        //notice that the vectors should be unit length
        public abstract void batchedUnitVectorGetSupportingVertexWithoutMargin(VInt3[] vectors,
            out VInt3[] supportVerticesOut);

        public abstract void getAabbSlow(VIntTransform t, out VInt3 aabbMin, out VInt3 aabbMax);

        public abstract int getNumPreferredPenetrationDirections();

        public abstract void getPreferredPenetrationDirection(int index, out VInt3 penetrationVector);

    }
}