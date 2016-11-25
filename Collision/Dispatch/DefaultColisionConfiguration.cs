using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class DefaultCollisionConfiguration: CollisionConfiguration
    {
        protected VoronoiSimplexSolver simplexSolver;
        protected ConvexPenetrationDepthSolver pdSolver;

        //default CreationFunctions, filling the m_doubleDispatch table
        protected CollisionAlgorithmCreateFunc convexConvexCreateFunc;
        protected CollisionAlgorithmCreateFunc convexConcaveCreateFunc;
        protected CollisionAlgorithmCreateFunc swappedConvexConcaveCreateFunc;
        protected CollisionAlgorithmCreateFunc compoundCreateFunc;
        protected CollisionAlgorithmCreateFunc swappedCompoundCreateFunc;
        protected CollisionAlgorithmCreateFunc emptyCreateFunc;
        protected CollisionAlgorithmCreateFunc sphereSphereCF;
        protected CollisionAlgorithmCreateFunc sphereBoxCF;
        protected CollisionAlgorithmCreateFunc boxSphereCF;
        protected CollisionAlgorithmCreateFunc boxBoxCF;
        protected CollisionAlgorithmCreateFunc sphereTriangleCF;
        protected CollisionAlgorithmCreateFunc triangleSphereCF;
        protected CollisionAlgorithmCreateFunc planeConvexCF;
        protected CollisionAlgorithmCreateFunc convexPlaneCF;

        public DefaultCollisionConfiguration()
        {
            simplexSolver = new VoronoiSimplexSolver();

            //#define USE_EPA 1
            //#ifdef USE_EPA
            pdSolver = new GjkEpaPenetrationDepthSolver();
            //#else
            //pdSolver = new MinkowskiPenetrationDepthSolver();
            //#endif//USE_EPA

            /*
            //default CreationFunctions, filling the m_doubleDispatch table
            */
            convexConvexCreateFunc = new ConvexConvexAlgorithm.CreateFunc(simplexSolver, pdSolver);
            emptyCreateFunc = new EmptyAlgorithm.CreateFunc();

            sphereSphereCF = new SphereSphereCollisionAlgorithm.CreateFunc();

            sphereBoxCF = new SphereBoxCollisionAlgorithm.CreateFunc();
            boxSphereCF = new SphereBoxCollisionAlgorithm.CreateFunc();
            boxSphereCF.swapped = true;
            /*m_sphereTriangleCF = new (mem)btSphereTriangleCollisionAlgorithm::CreateFunc;
            m_triangleSphereCF = new (mem)btSphereTriangleCollisionAlgorithm::CreateFunc;
            m_triangleSphereCF->m_swapped = true;
            */
            //boxBoxCF = new BoxBoxCollisionAlgorithm.CreateFunc();

            // convex versus plane
            //convexPlaneCF = new ConvexPlaneCollisionAlgorithm.CreateFunc();
            //planeConvexCF = new ConvexPlaneCollisionAlgorithm.CreateFunc();
            //planeConvexCF.swapped = true;
        }

        public override CollisionAlgorithmCreateFunc getCollisionAlgorithmCreateFunc(BroadphaseNativeType proxyType0, BroadphaseNativeType proxyType1)
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

            /*if ((proxyType0 == SPHERE_SHAPE_PROXYTYPE ) && (proxyType1==TRIANGLE_SHAPE_PROXYTYPE))
            {
                return	m_sphereTriangleCF;
            }

            if ((proxyType0 == TRIANGLE_SHAPE_PROXYTYPE  ) && (proxyType1==SPHERE_SHAPE_PROXYTYPE))
            {
                return	m_triangleSphereCF;
            }

            if ((proxyType0 == BroadphaseNativeType.BOX_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeType.BOX_SHAPE_PROXYTYPE)) {
                return boxBoxCF;
            }


            if (proxyType0 < BroadphaseNativeType.CONCAVE_SHAPES_START_HERE && (proxyType1 == BroadphaseNativeType.STATIC_PLANE_PROXYTYPE))
            {
                return convexPlaneCF;
            }

            if (proxyType1 < BroadphaseNativeType.CONCAVE_SHAPES_START_HERE && (proxyType0 == BroadphaseNativeType.STATIC_PLANE_PROXYTYPE))
            {
                return planeConvexCF;
            }*/

            if (proxyType0 < BroadphaseNativeType.CONCAVE_SHAPES_START_HERE && proxyType1 < BroadphaseNativeType.CONCAVE_SHAPES_START_HERE)
            {
                return convexConvexCreateFunc;
            }

            // failed to find an algorithm
            return emptyCreateFunc;
        }
    }
}