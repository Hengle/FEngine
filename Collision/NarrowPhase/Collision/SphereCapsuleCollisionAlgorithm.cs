using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public static class SphereCapsuleCollisionAlgorithm
    {
        public static void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, PersistentManifold resultOut)
        {
            bool needSwap = body0.getCollisionShape() is CapsuleShape;

            CollisionObject sphereObject = needSwap ? body1 : body0;
            CollisionObject capsuleObject = needSwap ? body0 : body1;

            SphereShape sphere = (SphereShape)sphereObject.getCollisionShape();
            CapsuleShape capsule = (CapsuleShape)capsuleObject.getCollisionShape();

            VInt3 spherePos = sphereObject.getWorldTransform().position;
            VIntTransform capsuleTransform = capsuleObject.getWorldTransform();
            VInt3 p0 = capsuleTransform.TransformPoint(capsule.getUpAxis() * capsule.getHalfHeight());
            VInt3 p1 = capsuleTransform.TransformPoint(capsule.getUpAxis() * -capsule.getHalfHeight());

            VFixedPoint param = VFixedPoint.Zero;
            VFixedPoint dist2 = Distance.distancePointSegmentSquared(p0, p1, spherePos, ref param);
            VFixedPoint dist = FMath.Sqrt(dist2);
            VFixedPoint penetration = dist - (sphere.getRadius() + capsule.getRadius());
            if(penetration > Globals.getContactBreakingThreshold())
            {
                return;
            }

            VInt3 lineWorldPos = p0 * (VFixedPoint.One - param) + p1 * param;
            VInt3 diff = (spherePos - lineWorldPos) / dist;

            VInt3 sphereWorldPos = spherePos - diff * sphere.getRadius();
            VInt3 capsuleWorldPos = lineWorldPos + diff * capsule.getRadius();

            ManifoldPoint contractPoint = new ManifoldPoint(needSwap ? capsuleWorldPos : sphereWorldPos, needSwap ? sphereWorldPos: capsuleWorldPos, diff * (needSwap ? -1 : 1), penetration);
            resultOut.addManifoldPoint(contractPoint);
        }
    }
}
