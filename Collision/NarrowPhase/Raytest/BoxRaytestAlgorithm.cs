using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public static class BoxRaytestAlgorithm
    {
        public static void rayTestSingle(VInt3 fromPos, VInt3 toPos, CollisionObject collisionObject, RayResultCallback resultCallback)
        {
            VIntTransform collisionObjectTransform = collisionObject.getWorldTransform();

            VFixedPoint t = VFixedPoint.Zero;
            VInt3 normal = VInt3.zero;
            VInt3 aabbMin, aabbMax;
            collisionObject.getCollisionShape().getAabb(VIntTransform.Identity, out aabbMin, out aabbMax);
            if(rayTestBox(fromPos, toPos, aabbMax, collisionObject.getWorldTransform(), ref t, ref normal))
            {
                resultCallback.addSingleResult(collisionObject, normal, t);
            }
        }

        public static bool rayTestBox(VInt3 fromPos, VInt3 toPos, VInt3 boxHalfExtent, VIntTransform boxTransform, ref VFixedPoint t, ref VInt3 normal)
        {
            VInt3 rayFromPointLocal = boxTransform.InverseTransformPoint(fromPos);
            VInt3 rayToPointLocal = boxTransform.InverseTransformPoint(toPos);

            t = VFixedPoint.One;
            VInt3 normalInLocal = VInt3.zero;

            if (AabbUtils.RayAabb(rayFromPointLocal, rayToPointLocal, -boxHalfExtent, boxHalfExtent, ref t, ref normalInLocal))
            {
                normal = boxTransform.TransformDirection(normalInLocal);
                return true;
            }

            return false;
        }
    }
}
