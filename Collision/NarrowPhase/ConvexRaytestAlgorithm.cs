using System;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class ConvexRaytestAlgorithm : RaytestAlgorithm
    {
        VoronoiSimplexSolver simplexSolver;
        SubsimplexConvexCast convexCaster;

        public ConvexRaytestAlgorithm()
        {
            VoronoiSimplexSolver simplexSolver = new VoronoiSimplexSolver();

            SubsimplexConvexCast convexCaster = new SubsimplexConvexCast(simplexSolver);
        }

        public override void rayTestSingle(VIntTransform rayFromTrans, VIntTransform rayToTrans, CollisionObject collisionObject, RayResultCallback resultCallback)
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

            simplexSolver.reset();

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
                        LocalRayResult localRayResult = new LocalRayResult(
                                collisionObject,
                                castResult.normal,
                                castResult.fraction);

                        bool normalInWorldSpace = true;
                        resultCallback.addSingleResult(localRayResult, normalInWorldSpace);
                    }
                }
            }
        }
    }
}
