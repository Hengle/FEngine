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

            VInt3 e = boxShape.getHalfExtentsWithoutMargin();
            VInt3 p0 = capsuleTransform.TransformPoint(capsuleShape.getUpAxis() * capsuleShape.getHalfHeight());
            VInt3 p1 = capsuleTransform.TransformPoint(capsuleShape.getUpAxis() * -capsuleShape.getHalfHeight());
            VFixedPoint radius = capsuleShape.getRadius();

            VInt3 boxParam = VInt3.zero; VFixedPoint lineParam = VFixedPoint.Zero;
            DistanceBox.distanceSegmentBoxSquared(p0, p1, e, boxTransform, ref lineParam, ref boxParam);

            VInt3 boxWorldPos = boxTransform.TransformPoint(boxParam);
            VInt3 lineWorldPos = p0 * (VFixedPoint.One - lineParam) + p1 * lineParam;

            VInt3 diff = boxWorldPos - lineWorldPos;
            VFixedPoint diffMagnitude = diff.magnitude;
            VFixedPoint distance = diffMagnitude - radius;

            
        }
    }
}
