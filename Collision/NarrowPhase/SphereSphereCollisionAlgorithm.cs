using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class SphereSphereCollisionAlgorithm: CollisionAlgorithm
    {

        public override void destroy()
        {

        }

        public override void processCollision(CollisionObject col0, CollisionObject col1, DispatcherInfo dispatchInfo,
            ManifoldResult resultOut)
        {
            SphereShape sphere0 = (SphereShape) col0.getCollisionShape();
            SphereShape sphere1 = (SphereShape) col1.getCollisionShape();

            VInt3 diff = col0.getWorldTransform().position - col1.getWorldTransform().position;

            VFixedPoint len = diff.magnitude;
            VFixedPoint radius0 = sphere0.getRadius();
            VFixedPoint radius1 = sphere1.getRadius();

            // if distance positive, don't generate a new contact
            if (len > (radius0 + radius1)) {
                return;
            }
            // distance (negative means penetration)
            VFixedPoint dist = len - (radius0 + radius1);

            VInt3 normalOnSurfaceB = new VInt3(VFixedPoint.One, VFixedPoint.Zero, VFixedPoint.Zero);
            if (len > Globals.EPS) {
                normalOnSurfaceB = diff/len;
            }

            // report a contact. internally this will be kept persistent, and contact reduction is done
            resultOut.addContactPoint(normalOnSurfaceB, dist);
        }
    }
}