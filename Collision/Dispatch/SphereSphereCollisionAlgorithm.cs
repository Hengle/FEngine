using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class SphereSphereCollisionAlgorithm: CollisionAlgorithm
    {
        private bool ownManifold;
        private PersistentManifold manifoldPtr;

        public void init(PersistentManifold mf, CollisionAlgorithmConstructionInfo ci, CollisionObject col0, CollisionObject col1)
        {
            base.init(ci);
            manifoldPtr = mf;

            if (manifoldPtr == null && dispatcher.needsCollision(col0, col1))
            {
                manifoldPtr = dispatcher.getNewManifold(col0, col1);
                ownManifold = true;
            }
        }

        public override void destroy()
        {
            if (ownManifold)
            {
                if (manifoldPtr != null)
                {
                    dispatcher.releaseManifold(manifoldPtr);
                }
                manifoldPtr = null;
            }
        }

        public override void processCollision(CollisionObject col0, CollisionObject col1, DispatcherInfo dispatchInfo,
            ManifoldResult resultOut)
        {
            if (manifoldPtr == null) {
                return;
            }

            resultOut.setPersistentManifold(manifoldPtr);

            SphereShape sphere0 = (SphereShape) col0.getCollisionShape();
            SphereShape sphere1 = (SphereShape) col1.getCollisionShape();

            VInt3 diff = col0.getWorldTransform().position - col1.getWorldTransform().position;

            VFixedPoint len = diff.magnitude;
            VFixedPoint radius0 = sphere0.getRadius();
            VFixedPoint radius1 = sphere1.getRadius();

            // if distance positive, don't generate a new contact
            if (len > (radius0 + radius1)) {
                resultOut.refreshContactPoints();
                return;
            }
            // distance (negative means penetration)
            VFixedPoint dist = len - (radius0 + radius1);

            VInt3 normalOnSurfaceB = new VInt3(VFixedPoint.One, VFixedPoint.Zero, VFixedPoint.Zero);
            if (len > Globals.EPS) {
                normalOnSurfaceB = diff/len;
            }

            // point on A (worldspace)
            VInt3 pos0 = col0.getWorldTransform().position - normalOnSurfaceB * radius0;

            // point on B (worldspace)
            VInt3 pos1 = col0.getWorldTransform().position + normalOnSurfaceB * radius1;

            // report a contact. internally this will be kept persistent, and contact reduction is done
            resultOut.addContactPoint(normalOnSurfaceB, pos1, dist);
            resultOut.refreshContactPoints();
        }

        public override VFixedPoint calculateTimeOfImpact(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ManifoldResult resultOut)
        {
            return VFixedPoint.One;
        }

        public override void getAllContactManifolds(List<PersistentManifold> manifoldArray)
        {
            if (manifoldPtr != null && ownManifold)
            {
                manifoldArray.Add(manifoldPtr);
            }
        }

        public class CreateFunc: CollisionAlgorithmCreateFunc
        {
            private ObjectPool<SphereSphereCollisionAlgorithm> pool = new ObjectPool<SphereSphereCollisionAlgorithm>();

            public override CollisionAlgorithm createCollisionAlgorithm(CollisionAlgorithmConstructionInfo ci, CollisionObject body0, CollisionObject body1) {
                SphereSphereCollisionAlgorithm algo = pool.Get();
                algo.init(null, ci, body0, body1);
                return algo;
            }

            public override void releaseCollisionAlgorithm(CollisionAlgorithm algo) {
                pool.Release((SphereSphereCollisionAlgorithm)algo);
            }
        };
    }
}