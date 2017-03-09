using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public static class SphereSphereCollisionAlgorithm
    {
        public static void processCollision(CollisionObject col0, CollisionObject col1, DispatcherInfo dispatchInfo,
            PersistentManifold resultOut)
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

            VInt3 normalOnSurfaceB = VInt3.zero;
            if (dist < Globals.EPS) {
                normalOnSurfaceB = diff/len;
            }

            // report a contact. internally this will be kept persistent, and contact reduction is done
            ManifoldPoint contactPoint = new ManifoldPoint(col0.getWorldTransform().position - normalOnSurfaceB * radius0, col1.getWorldTransform().position + normalOnSurfaceB * radius1, normalOnSurfaceB, dist);
            resultOut.addManifoldPoint(contactPoint);
        }
    }
}