using System.Collections.Generic;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public static class ConvexConvexSweepAlgorithm
    {
        static VoronoiSimplexSolver simplexSolver = new VoronoiSimplexSolver();
        static GjkConvexCast castPtr = new GjkConvexCast(simplexSolver);

        public static void objectQuerySingle(CollisionObject castObject, VInt3 ToPos, CollisionObject collisionObject, List<CastResult> results, VFixedPoint allowedPenetration)
        {
            CollisionShape collisionShape = collisionObject.getCollisionShape();
            VIntTransform colObjWorldTransform = collisionObject.getWorldTransform();
            CastResult castResult = new CastResult();
            castResult.hitObject = collisionObject;

            castPtr.convexA = castObject.getCollisionShape();
            castPtr.convexB = (ConvexShape)collisionShape;

            simplexSolver.reset();

            VIntTransform convexFromTrans = castObject.getWorldTransform();
            VIntTransform convexToTrans = convexFromTrans;
            convexToTrans.position = ToPos;

            if (castPtr.calcTimeOfImpact(convexFromTrans, convexToTrans, colObjWorldTransform, colObjWorldTransform, allowedPenetration, castResult))
            {
                //add hit
                results.Add(castResult);
            }
        }
    }
}
