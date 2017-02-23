using System.Collections.Generic;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public static class ConvexObjectQueryAlgorithm
    {
        static VoronoiSimplexSolver simplexSolver = new VoronoiSimplexSolver();
        static GjkConvexCast castPtr = new GjkConvexCast(simplexSolver);

        public static void objectQuerySingle(ConvexShape castShape, VIntTransform convexFromTrans, VIntTransform convexToTrans, CollisionObject collisionObject, List<CastResult> results, VFixedPoint allowedPenetration)
        {
            CollisionShape collisionShape = collisionObject.getCollisionShape();
            VIntTransform colObjWorldTransform = collisionObject.getWorldTransform();
            CastResult castResult = new CastResult();
            castResult.hitObject = collisionObject;
            castResult.allowedPenetration = allowedPenetration;

            castPtr.convexA = castShape;
            castPtr.convexB = (ConvexShape)collisionShape;

            simplexSolver.reset();

            if (castPtr.calcTimeOfImpact(convexFromTrans, convexToTrans, colObjWorldTransform, colObjWorldTransform, castResult))
            {
                //add hit
                results.Add(castResult);
            }
        }
    }
}
