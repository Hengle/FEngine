using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class BoxCapsuleCollisionAlgorithm: CollisionAlgorithm
    {
        public override void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ManifoldResult resultOut)
        {
            bool needSwap = body0.getCollisionShape() is CapsuleShape;
            CollisionObject boxObject = needSwap ? body1 : body0;
            CollisionObject capsuleObject = needSwap ? body0 : body1;

            BoxShape boxShape = (BoxShape)boxObject.getCollisionShape();
            CapsuleShape capsuleShape = (CapsuleShape)capsuleObject.getCollisionShape();

            VIntTransform boxTransform = boxObject.getWorldTransform();
            VIntTransform capsuleTransform = capsuleObject.getWorldTransform();

            VInt3 e = boxShape.getHalfExtentsWithoutMargin();
            VInt3 m = boxTransform.InverseTransformPoint(capsuleTransform.position);
            VInt3 d = boxTransform.InverseTransformVector(capsuleTransform.TransformVector(capsuleShape.getUpAxis() * capsuleShape.getHalfHeight()));
            VFixedPoint radius = capsuleShape.getRadius();

            VFixedPoint depth = VFixedPoint.MinValue;
            VFixedPoint currentDepth = VFixedPoint.Zero;
            VInt3 hitNormal = VInt3.zero;
   
            VFixedPoint adx = d.x.Abs();
            currentDepth = m.x.Abs() - (e.x + adx + radius);
            if (currentDepth > VFixedPoint.Zero)
                return;
            if(currentDepth > depth)
            {
                depth = currentDepth;
                hitNormal = boxTransform.right * (m[0] > VFixedPoint.Zero ? 1 : -1);
            }

            VFixedPoint ady = d.y.Abs();
            currentDepth = m.y.Abs() - (e.y + ady + radius);
            if (currentDepth > VFixedPoint.Zero)
                return;
            if (currentDepth > depth)
            {
                depth = currentDepth;
                hitNormal = boxTransform.up * (m[1] > VFixedPoint.Zero ? 1 : -1);
            }

            VFixedPoint adz = d.z.Abs();
            currentDepth = m.z.Abs() - (e.z + adz + radius);
            if (currentDepth > VFixedPoint.Zero)
                return;
            if (currentDepth > depth)
            {
                depth = currentDepth;
                hitNormal = boxTransform.forward * (m[2] > VFixedPoint.Zero ? 1 : -1);
            }

            VFixedPoint axis = m.y * d.z - m.z * d.y;
            currentDepth = axis.Abs() - (e.y * adz + e.z * ady + radius);
            if (currentDepth > VFixedPoint.Zero)
                return;
            if(currentDepth > depth)
            {
                depth = currentDepth;
                hitNormal = VInt3.Cross(d, VInt3.right) * (axis > VFixedPoint.Zero ? 1 : -1);
            }

            axis = m.z * d.x - m.x * d.z;
            currentDepth = axis.Abs() - (e.x * adz + e.z * adx + radius);
            if (currentDepth > VFixedPoint.Zero)
                return;
            if(currentDepth > depth)
            {
                depth = currentDepth;
                hitNormal = VInt3.Cross(d, VInt3.up) * (axis > VFixedPoint.Zero ? 1 : -1);
            }

            axis = m.x * d.y - m.y * d.x;
            currentDepth = axis.Abs() - (e.x * ady + e.y * adx + radius);
            if (currentDepth > VFixedPoint.Zero)
                return;
            if(currentDepth > depth)
            {
                depth = currentDepth;
                hitNormal = VInt3.Cross(d, VInt3.forward) * (axis > VFixedPoint.Zero ? 1 : -1);
            }

            hitNormal = boxTransform.TransformDirection(hitNormal) * (needSwap ? 1 : -1);
            resultOut.addContactPoint(hitNormal, depth); 
        }

        public override void destroy()
        {

        }
    }
}
