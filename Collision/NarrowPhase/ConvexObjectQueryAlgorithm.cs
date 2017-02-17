using System;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class ConvexObjectQueryAlgorithm : ObjectQueryAlgorithm
    {
        VoronoiSimplexSolver simplexSolver;
        ConvexCast castPtr;

        public ConvexObjectQueryAlgorithm()
        {
            simplexSolver = new VoronoiSimplexSolver();
            castPtr = new GjkConvexCast(simplexSolver);
        }

        public override void objectQuerySingle(ConvexShape castShape, VIntTransform convexFromTrans, VIntTransform convexToTrans, CollisionObject collisionObject, ConvexResultCallback resultCallback, VFixedPoint allowedPenetration)
        {
            CollisionShape collisionShape = collisionObject.getCollisionShape();
            VIntTransform colObjWorldTransform = collisionObject.getWorldTransform();
            CastResult castResult = new CastResult();
            castResult.allowedPenetration = allowedPenetration;
            castResult.fraction = resultCallback.m_closestHitFraction;

            castPtr.convexA = castShape;
            castPtr.convexB = (ConvexShape)collisionShape;

            simplexSolver.reset();

            if (castPtr.calcTimeOfImpact(convexFromTrans, convexToTrans, colObjWorldTransform, colObjWorldTransform, castResult))
            {
                //add hit
                if (castResult.normal.sqrMagnitude > Globals.EPS)
                {
                    if (castResult.fraction < resultCallback.m_closestHitFraction)
                    {
                        castResult.normal = castResult.normal.Normalize();
                        resultCallback.addSingleResult(collisionObject,
                            castResult.normal,
                            castResult.hitPoint,
                            castResult.fraction);

                    }
                }
            }
        }
    }
}
