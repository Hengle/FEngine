using System;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class DefaultCollisionConfiguration: CollisionConfiguration
    {
        //default CreationFunctions, filling the m_doubleDispatch table
        protected CollisionAlgorithm emptyCreateFunc;
        //protected CollisionAlgorithm convexConvexCreateFunc;
        protected CollisionAlgorithm sphereSphereCF;
        protected CollisionAlgorithm sphereBoxCF;
        protected CollisionAlgorithm boxBoxCF;
        protected CollisionAlgorithm boxCapsultCF;

        protected RaytestAlgorithm emptyRaytestFunc;
        //protected RaytestAlgorithm convexRaytestFunc;
        protected RaytestAlgorithm sphereRaytestFunc;
        protected RaytestAlgorithm boxRaytestFunc;
        protected RaytestAlgorithm capsuleRaytestFunc;

        protected SweepAlgorithm emptySweepFunc;
        //protected SweepAlgorithm convexConvexSweepFunc;
        protected SweepAlgorithm sphereSphereSweepFunc;
        protected SweepAlgorithm sphereBoxSweepFunc;
        protected SweepAlgorithm boxCapsultSweepFunc;
        protected SweepAlgorithm capsuleCapsuleSweepFunc;

        public DefaultCollisionConfiguration()
        {
            sphereSphereCF = SphereSphereCollisionAlgorithm.processCollision;
            sphereBoxCF = SphereBoxCollisionAlgorithm.processCollision;
            boxCapsultCF = BoxCapsuleCollisionAlgorithm.processCollision;
            boxBoxCF = BoxBoxCollisionAlgorithm.processCollision;
            //convexConvexCreateFunc = ConvexConvexAlgorithm.processCollision;
            emptyCreateFunc = EmptyAlgorithm.processCollision;

            emptyRaytestFunc = EmptyRaytestAlgorithm.rayTestSingle;
            //convexRaytestFunc = ConvexRaytestAlgorithm.rayTestSingle;
            sphereRaytestFunc = SphereRaytestAlgorithm.rayTestSingle;
            boxRaytestFunc = BoxRaytestAlgorithm.rayTestSingle;
            capsuleRaytestFunc = CapsuleRaytestAlgorithm.rayTestSingle;

            emptySweepFunc = EmptySweepFunc.objectQuerySingle;
            //convexConvexSweepFunc = ConvexConvexSweepAlgorithm.objectQuerySingle;
            sphereBoxSweepFunc = SphereBoxSweepAlgorithm.objectQuerySingle;
            sphereSphereSweepFunc = SphereSphereSweepAlgorithm.objectQuerySingle;
            capsuleCapsuleSweepFunc = CapsuleCapsuleSweepAlgorithm.objectQuerySingle;
            
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
                return sphereBoxCF;
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

            /*if (proxyType0 < BroadphaseNativeType.CONCAVE_SHAPES_START_HERE && proxyType1 < BroadphaseNativeType.CONCAVE_SHAPES_START_HERE)
            {
                return convexConvexCreateFunc;
            }*/

            // failed to find an algorithm
            return emptyCreateFunc;
        }

        public override RaytestAlgorithm getRaytestAlgorithm(BroadphaseNativeType proxyType)
        {
            if(proxyType == BroadphaseNativeType.SPHERE_SHAPE_PROXYTYPE)
            {
                return sphereRaytestFunc;
            }

            if (proxyType < BroadphaseNativeType.BOX_SHAPE_PROXYTYPE)
            {
                return boxRaytestFunc;
            }

            if (proxyType < BroadphaseNativeType.CAPSULE_SHAPE_PROXYTYPE)
            {
                return capsuleRaytestFunc;
            }

            /*if (proxyType < BroadphaseNativeType.CONCAVE_SHAPES_START_HERE)
            {
                return convexRaytestFunc;
            }*/

            return emptyRaytestFunc;
        }

        public override SweepAlgorithm getSweepAlgorithmCreateFunc(BroadphaseNativeType proxyType0, BroadphaseNativeType proxyType1)
        {
            if ((proxyType0 == BroadphaseNativeType.SPHERE_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeType.SPHERE_SHAPE_PROXYTYPE))
            {
                return sphereSphereSweepFunc;
            }

            if ((proxyType0 == BroadphaseNativeType.SPHERE_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeType.BOX_SHAPE_PROXYTYPE))
            {
                return sphereBoxSweepFunc;
            }

            if ((proxyType0 == BroadphaseNativeType.BOX_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeType.SPHERE_SHAPE_PROXYTYPE))
            {
                return sphereBoxSweepFunc;
            }

            if ((proxyType0 == BroadphaseNativeType.CAPSULE_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeType.CAPSULE_SHAPE_PROXYTYPE))
            {
                return capsuleCapsuleSweepFunc;
            }


            /*if (proxyType0 < BroadphaseNativeType.CONCAVE_SHAPES_START_HERE && proxyType1 < BroadphaseNativeType.CONCAVE_SHAPES_START_HERE)
            {
                return convexConvexSweepFunc;
            }*/

            // failed to find an algorithm
            return emptySweepFunc;
        }
    }
}