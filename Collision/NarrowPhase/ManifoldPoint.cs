using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class ManifoldPoint
    {
        public VInt3 localPointA;
        public VInt3 localPointB;
        public VInt3 positionWorldOnB;
        public VInt3 positionWorldOnA;
        public VInt3 normalWorldOnB;

        public VFixedPoint distance1;

        // BP mod, store contact triangles.
        public int partId0;
        public int partId1;
        public int index0;
        public int index1;

        public VFixedPoint appliedImpulse;

        public bool lateralFrictionInitialized;
        public VFixedPoint appliedImpulseLateral1;
        public VFixedPoint appliedImpulseLateral2;
        public int lifeTime; //lifetime of the contactpoint in frames

        public VInt3 lateralFrictionDir1 = new VInt3();
        public VInt3 lateralFrictionDir2 = new VInt3();

        public ManifoldPoint()
        {
            this.appliedImpulse = VFixedPoint.Zero;
            this.lateralFrictionInitialized = false;
            this.lifeTime = 0;
        }

        public ManifoldPoint(VInt3 pointA, VInt3 pointB, VInt3 normal, VFixedPoint distance)
        {
            init(pointA, pointB, normal, distance);
        }

        public void init(VInt3 pointA, VInt3 pointB, VInt3 normal, VFixedPoint distance)
        {
            this.localPointA = pointA;
            this.localPointB = pointB;
            this.normalWorldOnB = normal;
            this.distance1 = distance;
            this.appliedImpulse = VFixedPoint.Zero;
            this.lateralFrictionInitialized = false;
            this.appliedImpulseLateral1 = VFixedPoint.Zero;
            this.appliedImpulseLateral2 = VFixedPoint.Zero;
            this.lifeTime = 0;
        }

        public VFixedPoint getDistance()
        {
            return distance1;
        }

        public int getLifeTime()
        {
            return lifeTime;
        }

        public void set(ManifoldPoint p)
        {
            localPointA = p.localPointA;
            localPointB = p.localPointB;
            positionWorldOnA = p.positionWorldOnA;
            positionWorldOnB = p.positionWorldOnB;
            normalWorldOnB = p.normalWorldOnB;
            distance1 = p.distance1;
            partId0 = p.partId0;
            partId1 = p.partId1;
            index0 = p.index0;
            index1 = p.index1;
            appliedImpulse = p.appliedImpulse;
            lateralFrictionInitialized = p.lateralFrictionInitialized;
            appliedImpulseLateral1 = p.appliedImpulseLateral1;
            appliedImpulseLateral2 = p.appliedImpulseLateral2;
            lifeTime = p.lifeTime;
            lateralFrictionDir1 = p.lateralFrictionDir1;
            lateralFrictionDir2 = p.lateralFrictionDir2;
        }

        public VInt3 getPositionWorldOnA()
        {
            return positionWorldOnA;
            //return m_positionWorldOnB + m_normalWorldOnB * m_distance1;
        }

        public VInt3 getPositionWorldOnB()
        {
            return positionWorldOnB;
        }

        public void setDistance(VFixedPoint dist)
        {
            distance1 = dist;
        }
    }
}
