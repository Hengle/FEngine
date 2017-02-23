﻿using System;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class DefaultCollisionConfiguration: CollisionConfiguration
    {
        //default CreationFunctions, filling the m_doubleDispatch table
        protected CollisionAlgorithm emptyCreateFunc;
        protected CollisionAlgorithm convexConvexCreateFunc;
        protected CollisionAlgorithm sphereSphereCF;
        protected CollisionAlgorithm sphereBoxCF;
        protected CollisionAlgorithm boxBoxCF;
        protected CollisionAlgorithm boxCapsultCF;

        protected RaytestAlgorithm emptyRaytestFunc;
        protected RaytestAlgorithm convexRaytestFunc;
        protected RaytestAlgorithm sphereRaytestFunc;
        protected RaytestAlgorithm boxRaytestFunc;

        public DefaultCollisionConfiguration()
        {
            sphereSphereCF = SphereSphereCollisionAlgorithm.processCollision;
            sphereBoxCF = SphereBoxCollisionAlgorithm.processCollision;
            boxCapsultCF = BoxCapsuleCollisionAlgorithm.processCollision;
            boxBoxCF = BoxBoxCollisionAlgorithm.processCollision;
            convexConvexCreateFunc = ConvexConvexAlgorithm.processCollision;
            emptyCreateFunc = EmptyAlgorithm.processCollision;

            emptyRaytestFunc = EmptyRaytestAlgorithm.rayTestSingle;
            convexRaytestFunc = ConvexRaytestAlgorithm.rayTestSingle;
            sphereRaytestFunc = SphereRaytestAlgorithm.rayTestSingle;
            boxRaytestFunc = BoxRaytestAlgorithm.rayTestSingle;
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

            if (proxyType < BroadphaseNativeType.BOX_SHAPE_PROXYTYPE)
            {
                return boxRaytestFunc;
            }

            if (proxyType < BroadphaseNativeType.CONCAVE_SHAPES_START_HERE)
            {
                return convexRaytestFunc;
            }

            return emptyRaytestFunc;
        }
    }
}