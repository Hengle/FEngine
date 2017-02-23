using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public static class CapsuleRaytestAlgorithm
    {
        public static void rayTestSingle(VInt3 fromPos, VInt3 toPos, CollisionObject collisionObject, RayResultCallback resultCallback)
        {
            CapsuleShape capsule = (CapsuleShape)collisionObject.getCollisionShape();
            VInt3 p0 = collisionObject.getWorldTransform().TransformPoint(capsule.getUpAxis() * capsule.getHalfHeight());
            VInt3 p1 = collisionObject.getWorldTransform().TransformPoint(capsule.getUpAxis() * -capsule.getHalfHeight());
            VInt3 capsDir = p0 - p1;
            VFixedPoint kW = capsDir.magnitude;

            if(kW < Globals.EPS)
            {
                VFixedPoint t0 = VFixedPoint.Zero;
                VInt3 hitNormal = VInt3.zero;
                if(SphereRaytestAlgorithm.rayTestSphere(fromPos, toPos, collisionObject.getWorldTransform().position, capsule.getRadius(), ref hitNormal, ref t0 ))
                {
                    resultCallback.addSingleResult(collisionObject, hitNormal.Normalize(), t0);
                }
                else
                {
                    return;
                }
            }
            

        }
    }
}
