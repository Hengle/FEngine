using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public static class CapsuleCapsuleCollisionAlgorithm
    {
        public static void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, PersistentManifold resultOut)
        {
            CapsuleShape capsule0 = (CapsuleShape)body0.getCollisionShape();
            CapsuleShape capsule1 = (CapsuleShape)body1.getCollisionShape();

            VIntTransform transform0 = body0.getWorldTransform();
            VIntTransform transform1 = body1.getWorldTransform();

            VInt3 p00 = transform0.TransformPoint(capsule0.getUpAxis() * capsule0.getHalfHeight());
            VInt3 p01 = transform0.TransformPoint(capsule0.getUpAxis() * -capsule0.getHalfHeight());

            VInt3 p10 = transform1.TransformPoint(capsule1.getUpAxis() * capsule1.getHalfHeight());
            VInt3 p11 = transform1.TransformPoint(capsule1.getUpAxis() * -capsule1.getHalfHeight());

            VInt3 x, y;
            VFixedPoint dist2 = Distance.SegmentSegmentDist2(p00, p01 - p00, p10, p11 - p10, out x, out y);
            VFixedPoint dist = FMath.Sqrt(dist2);
            VFixedPoint penetration = dist - (capsule0.getRadius() + capsule1.getRadius());
            
            if(penetration > Globals.getContactBreakingThreshold())
            {
                return;
            }

            VInt3 diff = (x - y) / dist;

            VInt3 posWorldOnA = x - diff * capsule0.getRadius();
            VInt3 posWorldOnB = y + diff * capsule1.getRadius();

            ManifoldPoint contractPoint = new ManifoldPoint(posWorldOnA, posWorldOnB, diff, penetration);

            resultOut.addManifoldPoint(contractPoint);
        }
    }
}
