using System;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class DefaultCollisionConfiguration: CollisionConfiguration
    {
        protected VoronoiSimplexSolver simplexSolver;
        protected ConvexPenetrationDepthSolver pdSolver;

        //default CreationFunctions, filling the m_doubleDispatch table
        protected CollisionAlgorithm emptyCreateFunc;
        protected CollisionAlgorithm convexConvexCreateFunc;
        protected CollisionAlgorithm sphereSphereCF;
        protected CollisionAlgorithm sphereBoxCF;
        protected CollisionAlgorithm boxSphereCF;
        protected CollisionAlgorithm boxBoxCF;
        protected CollisionAlgorithm boxCapsultCF;

        protected RaytestAlgorithm emptyRaytestFunc;
        protected RaytestAlgorithm convexRaytestFunc;
        protected RaytestAlgorithm sphereRaytestFunc;

        protected ObjectQueryAlgorithm emptyOjbectQueryFunc;
        protected ObjectQueryAlgorithm convexObjectQueryFunc;
        public DefaultCollisionConfiguration()
        {
            simplexSolver = new VoronoiSimplexSolver();
            pdSolver = new EpaSolver();

            /*
            //default CreationFunctions, filling the m_doubleDispatch table
            */
            convexConvexCreateFunc = new ConvexConvexAlgorithm(simplexSolver, pdSolver);
            emptyCreateFunc = new EmptyAlgorithm();

            sphereSphereCF = new SphereSphereCollisionAlgorithm();
            sphereBoxCF = new SphereBoxCollisionAlgorithm();
            boxSphereCF = new SphereBoxCollisionAlgorithm();
            boxCapsultCF = new BoxCapsuleCollisionAlgorithm();
            boxBoxCF = new BoxBoxCollisionAlgorithm();

            emptyRaytestFunc = new EmptyRaytestAlgorithm();
            convexRaytestFunc = new ConvexRaytestAlgorithm();
            sphereRaytestFunc = new SphereRaytestAlgorithm();

            emptyOjbectQueryFunc = new EmptyObjectQueryFunc();
            convexObjectQueryFunc = new ConvexObjectQueryAlgorithm();
        }

        public override CollisionAlgorithm getCollisionAlgorithmCreateFunc(BroadphaseNativeType proxyType0, BroadphaseNativeType proxyType1)
        {
            if ((proxyType0 == BroadphaseNativeType.SPHERE_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeType.SPHERE_SHAPE_PROXYTYPE))
            {
                return sphereSphereCF;
            }

            if ((proxyType0 == BroadphaseNativeType.SPHERE_SHAPE_PROXYTYPE) && (proxyType1==BroadphaseNativeType.BOX_SHAPE_PROXYTYPE))
            {
                return	sphereBoxCF;
            }

            if ((proxyType0 == BroadphaseNativeType.BOX_SHAPE_PROXYTYPE ) && (proxyType1==BroadphaseNativeType.SPHERE_SHAPE_PROXYTYPE))
            {
                return	boxSphereCF;
            }

            if ((proxyType0 == BroadphaseNativeType.BOX_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeType.BOX_SHAPE_PROXYTYPE)) {
                return boxBoxCF;
            }

            if ((proxyType0 == BroadphaseNativeType.BOX_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeType.CAPSULE_SHAPE_PROXYTYPE))
            {
                return boxCapsultCF;
            }

            if ((proxyType0 == BroadphaseNativeType.CAPSULE_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeType.BOX_SHAPE_PROXYTYPE))
            {
                return boxCapsultCF;
            }

            if (proxyType0 < BroadphaseNativeType.CONCAVE_SHAPES_START_HERE && proxyType1 < BroadphaseNativeType.CONCAVE_SHAPES_START_HERE)
            {
                return convexConvexCreateFunc;
            }

            // failed to find an algorithm
            return emptyCreateFunc;
        }

        public override RaytestAlgorithm getRaytestAlgorithm(BroadphaseNativeType proxyType)
        {
            if(proxyType == BroadphaseNativeType.SPHERE_SHAPE_PROXYTYPE)
            {
                return sphereRaytestFunc;
            }

            if (proxyType < BroadphaseNativeType.CONCAVE_SHAPES_START_HERE)
            {
                return convexRaytestFunc;
            }

            return emptyRaytestFunc;
        }

        public override ObjectQueryAlgorithm getObjectQueryAlgorithm(BroadphaseNativeType proxyType0, BroadphaseNativeType proxyType1)
        {
            if (proxyType0 < BroadphaseNativeType.CONCAVE_SHAPES_START_HERE && proxyType1 < BroadphaseNativeType.CONCAVE_SHAPES_START_HERE)
            {
                return convexObjectQueryFunc;
            }

            return emptyOjbectQueryFunc;
        }
    }
}