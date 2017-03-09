using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public static class BoxCapsuleCollisionAlgorithm
    {
        public static void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, PersistentManifold resultOut)
        {
            bool needSwap = body0.getCollisionShape() is CapsuleShape;
            CollisionObject boxObject = needSwap ? body1 : body0;
            CollisionObject capsuleObject = needSwap ? body0 : body1;

            BoxShape boxShape = (BoxShape)boxObject.getCollisionShape();
            CapsuleShape capsuleShape = (CapsuleShape)capsuleObject.getCollisionShape();

            VIntTransform boxTransform = boxObject.getWorldTransform();
            VIntTransform capsuleTransform = capsuleObject.getWorldTransform();

            VInt3 p0 = capsuleTransform.TransformPoint(capsuleShape.getUpAxis() * capsuleShape.getHalfHeight()), p1 = capsuleTransform.TransformPoint(capsuleShape.getUpAxis() * -capsuleShape.getHalfHeight());
            VFixedPoint lParam = VFixedPoint.Zero; VInt3 closestPointBoxWS = VInt3.zero;
            VFixedPoint distSq = SegmentBoxDistance.distanceSegmentBoxSquared(p0, p1, boxShape.getHalfExtentsWithoutMargin(), boxTransform, ref lParam, ref closestPointBoxWS);
            VInt3 closestPointLineWS = p0 * (VFixedPoint.One - lParam) + p1 * lParam;

            VFixedPoint dist = FMath.Sqrt(distSq) - capsuleShape.getRadius();

            if(closestPointBoxWS != closestPointLineWS)
            {
                VInt3 normalOnBoxWS = (closestPointLineWS - closestPointBoxWS).Normalize();

                ManifoldPoint contactPoint = new ManifoldPoint(needSwap ? closestPointLineWS - normalOnBoxWS * capsuleShape.getRadius() : closestPointBoxWS,
                    !needSwap ? closestPointLineWS - normalOnBoxWS * capsuleShape.getRadius() : closestPointBoxWS,
                    normalOnBoxWS * (needSwap ? 1 : -1), dist); 
            }
            else //box and line are intersected
            {
                int reflection = 1;
                int axis = 0;
                VFixedPoint largestValue = VFixedPoint.Zero;
                VInt3 boxToPoint = boxTransform.InverseTransformPoint(closestPointBoxWS);
                for(int i = 0; i < 3; i++)
                {
                    VFixedPoint val = boxToPoint[i];
                    int localReflection = 1;
                    if(val < VFixedPoint.Zero)
                    {
                        localReflection = -1;
                        val *= -1;
                    }
                    if(val > largestValue)
                    {
                        reflection = localReflection;
                        axis = i;
                        largestValue = val;
                    }
                }

                VInt3 normal = VInt3.zero; normal[axis] = VFixedPoint.One * reflection;
                normal = boxTransform.TransformDirection(normal);

                ManifoldPoint contactPoint = new ManifoldPoint(needSwap ? closestPointLineWS - normal * capsuleShape.getRadius() : closestPointBoxWS,
                    !needSwap ? closestPointLineWS - normal * capsuleShape.getRadius() : closestPointBoxWS,
                    normal * (needSwap ? 1 : -1), dist);
            }
        }
    }
}
