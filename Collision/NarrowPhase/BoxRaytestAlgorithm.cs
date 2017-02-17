using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class BoxRaytestAlgorithm : RaytestAlgorithm
    {
        public override void rayTestSingle(VIntTransform rayFromTrans, VIntTransform rayToTrans, CollisionObject collisionObject, RayResultCallback resultCallback)
        {
            VIntTransform collisionObjectTransform = collisionObject.getWorldTransform();
            VInt3 rayFromPointLocal = collisionObjectTransform.InverseTransformPoint(rayFromTrans.position);
            VInt3 rayToPointLocal = collisionObjectTransform.InverseTransformPoint(rayToTrans.position);

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
