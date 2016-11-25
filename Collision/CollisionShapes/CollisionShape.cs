using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public abstract class CollisionShape
    {
        public abstract void getAabb(VIntTransform t, out VInt3 aabbMin, out VInt3 aabbMax);

        public void getBoundingSphere(out VInt3 center, out VFixedPoint radius) {
            VIntTransform tr = VIntTransform.Identity;
            VInt3 aabbMin, aabbMax;

            getAabb(tr, out aabbMin, out aabbMax);

            radius = (aabbMax - aabbMin).magnitude * VFixedPoint.Half;
            center = (aabbMax + aabbMin) * VFixedPoint.Half;
        }

        ///getAngularMotionDisc returns the maximus radius needed for Conservative Advancement to handle time-of-impact with rotations.
        public VFixedPoint getAngularMotionDisc()
        {
            VInt3 center;
            VFixedPoint disc;
            getBoundingSphere(out center, out disc);
            disc += center.magnitude;
            return disc;
        }

        ///calculateTemporalAabb calculates the enclosing aabb for the moving object over interval [0..timeStep)
        ///result is conservative
        public void calculateTemporalAabb(VIntTransform curTrans, VInt3 linvel, VInt3 angvel, VFixedPoint timeStep, out VInt3 temporalAabbMin, out VInt3 temporalAabbMax)
        {
            //start with static aabb
            getAabb(curTrans, out temporalAabbMin, out temporalAabbMax);

            VFixedPoint temporalAabbMaxx = temporalAabbMax.x;
            VFixedPoint temporalAabbMaxy = temporalAabbMax.y;
            VFixedPoint temporalAabbMaxz = temporalAabbMax.z;
            VFixedPoint temporalAabbMinx = temporalAabbMin.x;
            VFixedPoint temporalAabbMiny = temporalAabbMin.y;
            VFixedPoint temporalAabbMinz = temporalAabbMin.z;

            // add linear motion
            VInt3 linMotion = linvel * timeStep;

            if (linMotion.x > VFixedPoint.Zero) {
                temporalAabbMaxx += linMotion.x;
            }
            else {
                temporalAabbMinx += linMotion.x;
            }
            if (linMotion.y > VFixedPoint.Zero) {
                temporalAabbMaxy += linMotion.y;
            }
            else {
                temporalAabbMiny += linMotion.y;
            }
            if (linMotion.z > VFixedPoint.Zero) {
                temporalAabbMaxz += linMotion.z;
            }
            else {
                temporalAabbMinz += linMotion.z;
            }

            //add conservative angular motion
            VFixedPoint angularMotion = angvel.magnitude * getAngularMotionDisc() * timeStep;
            VInt3 angularMotion3d = new VInt3(angularMotion, angularMotion, angularMotion);
            temporalAabbMin = new VInt3(temporalAabbMinx, temporalAabbMiny, temporalAabbMinz);
            temporalAabbMax = new VInt3(temporalAabbMaxx, temporalAabbMaxy, temporalAabbMaxz);

            temporalAabbMin -= angularMotion3d;
            temporalAabbMax += angularMotion3d;
        }

        public bool isPolyhedral() {
            return getShapeType() < BroadphaseNativeType.IMPLICIT_CONVEX_SHAPES_START_HERE ;
        }

        public bool isConvex() {
            return getShapeType() < BroadphaseNativeType.CONCAVE_SHAPES_START_HERE;
        }

        public bool isConcave() {
            return getShapeType() > BroadphaseNativeType.CONCAVE_SHAPES_START_HERE &&
                getShapeType() < BroadphaseNativeType.CONCAVE_SHAPES_END_HERE;
        }

        public abstract void setLocalScaling(VInt3 scaling);

        public abstract VInt3 getLocalScaling();

        public abstract void calculateLocalInertia(VFixedPoint mass, out VInt3 inertia);

        public abstract BroadphaseNativeType getShapeType();

        public abstract void setMargin(VFixedPoint margin);

        public abstract VFixedPoint getMargin();

    }
}