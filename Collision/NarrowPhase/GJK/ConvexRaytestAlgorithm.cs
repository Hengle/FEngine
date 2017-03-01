using System;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public static class ConvexRaytestAlgorithm
    {
        static VoronoiSimplexSolver simplexSolver = new VoronoiSimplexSolver();
        static SubsimplexConvexCast convexCaster = new SubsimplexConvexCast(simplexSolver);

        public static void rayTestSingle(VInt3 fromPos, VInt3 toPos, CollisionObject collisionObject, RayResultCallback resultCallback)
        {
            CollisionShape collisionShape = collisionObject.getCollisionShape();
            VIntTransform colObjWorldTransform = collisionObject.getWorldTransform();
            SphereShape pointShape = new SphereShape(VFixedPoint.Zero);
            pointShape.setMargin(VFixedPoint.Zero);
            ConvexShape castShape = pointShape;
            ConvexShape convexShape = (ConvexShape)collisionShape;

            CastResult castResult = new CastResult();
            castResult.fraction = resultCallback.closestHitFraction;

            convexCaster.convexA = castShape;
            convexCaster.convexB = convexShape;

            VIntTransform rayFromTrans = VIntTransform.Identity;
            rayFromTrans.position = fromPos;
            VIntTransform rayToTrans = VIntTransform.Identity;
            rayToTrans.position = toPos;

            if (convexCaster.calcTimeOfImpact(rayFromTrans, rayToTrans, colObjWorldTransform, colObjWorldTransform, castResult))
            {
                //add hit
                if (castResult.normal.sqrMagnitude > VFixedPoint.Zero)
                {
                    if (castResult.fraction < resultCallback.closestHitFraction)
                    {
                        //rotate normal into worldspace
                        rayFromTrans.TransformVector(castResult.normal);

                        castResult.normal = castResult.normal.Normalize();

                        resultCallback.addSingleResult(collisionObject,
                                castResult.normal,
                                castResult.fraction);
                    }
                }
            }
        }
    }
}
