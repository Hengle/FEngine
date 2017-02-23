using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public static class BoxRaytestAlgorithm
    {
        public static void rayTestSingle(VInt3 fromPos, VInt3 toPos, CollisionObject collisionObject, RayResultCallback resultCallback)
        {
            VIntTransform collisionObjectTransform = collisionObject.getWorldTransform();
            VInt3 rayFromPointLocal = collisionObjectTransform.InverseTransformPoint(fromPos);
            VInt3 rayToPointLocal = collisionObjectTransform.InverseTransformPoint(toPos);

            BoxShape boxShape = (BoxShape)collisionObject.getCollisionShape();

            VFixedPoint t = VFixedPoint.One;
            VInt3 normalInLocal = VInt3.zero;

            if(AabbUtils.RayAabb(rayFromPointLocal, rayToPointLocal, -boxShape.getHalfExtentsWithoutMargin(), boxShape.getHalfExtentsWithoutMargin(), ref t, ref normalInLocal))
            {
                VInt3 normal = collisionObjectTransform.TransformDirection(normalInLocal);
                resultCallback.addSingleResult(collisionObject, normal, t);
            }
        }
    }
}
