using System;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class SphereRaytestAlgorithm : RaytestAlgorithm
    {
        public override void rayTestSingle(VIntTransform rayFromTrans, VIntTransform rayToTrans, CollisionObject collisionObject, RayResultCallback resultCallback)
        {
            SphereShape sphereShape = (SphereShape)collisionObject.getCollisionShape();
            VInt3 objectPosition = collisionObject.getWorldTransform().position;
            if((objectPosition - rayFromTrans.position).sqrMagnitude < sphereShape.getRadius())
            {
                //if start point in sphere, not contact
                return;
            }

            VInt3 d = (rayToTrans.position - rayFromTrans.position).Normalize();
            VInt3 p = rayFromTrans.position;

            VFixedPoint tm = -VInt3.Dot(p, d);
            VFixedPoint lm2 = VInt3.Dot(p, p) - tm * tm;
            VFixedPoint deltaT = FMath.Sqrt(VFixedPoint.One - lm2);
            VFixedPoint t0 = tm + deltaT;
            if(t0 > VFixedPoint.Zero && t0 < VFixedPoint.One)
            {
                VInt3 contactPoint = p + d * t0;
                VInt3 hitNormal = contactPoint - objectPosition;
                resultCallback.addSingleResult(collisionObject, hitNormal, t0);
            }
        }
    }
}
