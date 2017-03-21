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
            VFixedPoint lParam = VFixedPoint.Zero; VInt3 closestPointBoxLS = VInt3.zero;
            VFixedPoint distSq = SegmentBoxDistance.distanceSegmentBoxSquared(p0, p1, boxShape.getHalfExtent(), boxTransform, ref lParam, ref closestPointBoxLS);
            VInt3 closestPointBoxWS = boxTransform.TransformPoint(closestPointBoxLS);
            VInt3 closestPointLineWS = p0 * (VFixedPoint.One - lParam) + p1 * lParam;

            VFixedPoint dist = FMath.Sqrt(distSq) - capsuleShape.getRadius();

            if (dist > VFixedPoint.Zero)
                return;

            if((closestPointBoxWS - closestPointLineWS).sqrMagnitude > Globals.EPS2)
            {
                VInt3 normalOnBoxWS = (closestPointLineWS - closestPointBoxWS).Normalize();

                ManifoldPoint contactPoint = new ManifoldPoint(needSwap ? closestPointLineWS - normalOnBoxWS * capsuleShape.getRadius() : closestPointBoxWS,
                    !needSwap ? closestPointLineWS - normalOnBoxWS * capsuleShape.getRadius() : closestPointBoxWS,
                    normalOnBoxWS * (needSwap ? 1 : -1), dist);
                resultOut.addManifoldPoint(contactPoint);
            }
            else //box and line are intersected
            {
                //EPA
                LineShape coreShape = new LineShape(capsuleShape);
                VInt3 pa = VInt3.zero, pb = VInt3.zero, normal = VInt3.zero;
                VFixedPoint depth = VFixedPoint.Zero;
                PxGJKStatus result = EpaSolver.calcPenDepth(coreShape, boxShape, capsuleTransform, boxTransform, ref pa, ref pb, ref normal, ref depth);
                if (result == PxGJKStatus.EPA_CONTACT)
                {
                    ManifoldPoint contactPoint = new ManifoldPoint(needSwap ? pa - normal * capsuleShape.getRadius() : pb, !needSwap ? pa - normal * capsuleShape.getRadius() : pb, needSwap ? normal : -normal, depth - capsuleShape.getRadius());
                    resultOut.addManifoldPoint(contactPoint);
                }
            }
        }

    }
}
