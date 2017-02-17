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

        protected RaytestAlgorithm emptyRaytestFunc;
        protected RaytestAlgorithm convexRaytestFunc;
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
            /*m_sphereTriangleCF = new (mem)btSphereTriangleCollisionAlgorithm::CreateFunc;
            m_triangleSphereCF = new (mem)btSphereTriangleCollisionAlgorithm::CreateFunc;
            m_triangleSphereCF->m_swapped = true;
            */
            boxBoxCF = new BoxBoxCollisionAlgorithm();

            // convex versus plane
            //convexPlaneCF = new ConvexPlaneCollisionAlgorithm.CreateFunc();
            //planeConvexCF = new ConvexPlaneCollisionAlgorithm.CreateFunc();
            //planeConvexCF.swapped = true;
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

            if (proxyType0 < BroadphaseNativeType.CONCAVE_SHAPES_START_HERE && proxyType1 < BroadphaseNativeType.CONCAVE_SHAPES_START_HERE)
            {
                return convexConvexCreateFunc;
            }

            // failed to find an algorithm
            return emptyCreateFunc;
        }

        public override RaytestAlgorithm getRaytestAlgorithm(BroadphaseNativeType proxyType)
        {
            if (proxyType < BroadphaseNativeType.CONCAVE_SHAPES_START_HERE)
            {
                return convexRaytestFunc;
            }

            return emptyRaytestFunc;
        }
    }
}