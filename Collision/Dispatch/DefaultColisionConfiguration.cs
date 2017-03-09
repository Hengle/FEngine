using System;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class DefaultCollisionConfiguration: CollisionConfiguration
    {
        protected CollisionAlgorithm emptyCreateFunc;
        protected CollisionAlgorithm sphereSphereCF;
        protected CollisionAlgorithm sphereCapsuleCF;
        protected CollisionAlgorithm sphereBoxCF;
        protected CollisionAlgorithm capsuleCapsuleCF;
        protected CollisionAlgorithm boxCapsuleCF;
        protected CollisionAlgorithm boxBoxCF;

        protected RaytestAlgorithm emptyRaytestFunc;
        protected RaytestAlgorithm sphereRaytestFunc;
        protected RaytestAlgorithm boxRaytestFunc;
        protected RaytestAlgorithm capsuleRaytestFunc;

        protected SweepAlgorithm emptySweepFunc;
        protected SweepAlgorithm sphereSphereSweepFunc;
        protected SweepAlgorithm sphereCapsuleSweepFunc;
        protected SweepAlgorithm sphereBoxSweepFunc;
        protected SweepAlgorithm capsuleCapsuleSweepFunc;
        protected SweepAlgorithm capsuleBoxSweepFunc;

        public DefaultCollisionConfiguration()
        {
            sphereSphereCF = SphereSphereCollisionAlgorithm.processCollision;
            sphereBoxCF = SphereBoxCollisionAlgorithm.processCollision;
            sphereCapsuleCF = SphereCapsuleCollisionAlgorithm.processCollision;
            capsuleCapsuleCF = CapsuleCapsuleCollisionAlgorithm.processCollision;
            boxCapsuleCF = BoxCapsuleCollisionAlgorithm.processCollision;
            boxBoxCF = BoxBoxCollisionAlgorithm.processCollision;
            emptyCreateFunc = EmptyAlgorithm.processCollision;

            emptyRaytestFunc = EmptyRaytestAlgorithm.rayTestSingle;
            sphereRaytestFunc = SphereRaytestAlgorithm.rayTestSingle;
            boxRaytestFunc = BoxRaytestAlgorithm.rayTestSingle;
            capsuleRaytestFunc = CapsuleRaytestAlgorithm.rayTestSingle;

            emptySweepFunc = EmptySweepFunc.objectQuerySingle;
            sphereBoxSweepFunc = SphereBoxSweepAlgorithm.objectQuerySingle;
            sphereSphereSweepFunc = SphereSphereSweepAlgorithm.objectQuerySingle;
            sphereCapsuleSweepFunc = SphereCapsuleSweepAlgorithm.objectQuerySingle;
            capsuleCapsuleSweepFunc = CapsuleCapsuleSweepAlgorithm.objectQuerySingle;
            capsuleBoxSweepFunc = CapsuleBoxSweepAlgorithm.objectQuerySingle;
            
        }

        public override CollisionAlgorithm getCollisionAlgorithmCreateFunc(BroadphaseNativeType proxyType0, BroadphaseNativeType proxyType1)
        {
            if ((proxyType0 == BroadphaseNativeType.SPHERE_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeType.SPHERE_SHAPE_PROXYTYPE))
            {
                return sphereSphereCF;
            }

            if ((proxyType0 == BroadphaseNativeType.SPHERE_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeType.CAPSULE_SHAPE_PROXYTYPE))
            {
                return sphereCapsuleCF;
            }

            if ((proxyType0 == BroadphaseNativeType.CAPSULE_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeType.SPHERE_SHAPE_PROXYTYPE))
            {
                return sphereCapsuleCF;
            }

            if ((proxyType0 == BroadphaseNativeType.SPHERE_SHAPE_PROXYTYPE) && (proxyType1==BroadphaseNativeType.BOX_SHAPE_PROXYTYPE))
            {
                return	sphereBoxCF;
            }

            if ((proxyType0 == BroadphaseNativeType.BOX_SHAPE_PROXYTYPE ) && (proxyType1==BroadphaseNativeType.SPHERE_SHAPE_PROXYTYPE))
            {
                return sphereBoxCF;
            }

            if ((proxyType0 == BroadphaseNativeType.CAPSULE_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeType.CAPSULE_SHAPE_PROXYTYPE))
            {
                return capsuleCapsuleCF;
            }

            if ((proxyType0 == BroadphaseNativeType.BOX_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeType.CAPSULE_SHAPE_PROXYTYPE))
            {
                return boxCapsuleCF;
            }

            if ((proxyType0 == BroadphaseNativeType.CAPSULE_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeType.BOX_SHAPE_PROXYTYPE))
            {
                return boxCapsuleCF;
            }

            if ((proxyType0 == BroadphaseNativeType.BOX_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeType.BOX_SHAPE_PROXYTYPE)) {
                return boxBoxCF;
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

            if (proxyType < BroadphaseNativeType.CAPSULE_SHAPE_PROXYTYPE)
            {
                return capsuleRaytestFunc;
            }

            return emptyRaytestFunc;
        }

        public override SweepAlgorithm getSweepAlgorithmCreateFunc(BroadphaseNativeType proxyType0, BroadphaseNativeType proxyType1)
        {
            if ((proxyType0 == BroadphaseNativeType.SPHERE_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeType.SPHERE_SHAPE_PROXYTYPE))
            {
                return sphereSphereSweepFunc;
            }

            if ((proxyType0 == BroadphaseNativeType.CAPSULE_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeType.SPHERE_SHAPE_PROXYTYPE))
            {
                return sphereCapsuleSweepFunc;
            }

            if ((proxyType0 == BroadphaseNativeType.SPHERE_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeType.CAPSULE_SHAPE_PROXYTYPE))
            {
                return sphereCapsuleSweepFunc;
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

            if ((proxyType0 == BroadphaseNativeType.CAPSULE_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeType.BOX_SHAPE_PROXYTYPE))
            {
                return capsuleBoxSweepFunc;
            }

            if ((proxyType0 == BroadphaseNativeType.BOX_SHAPE_PROXYTYPE) && (proxyType1 == BroadphaseNativeType.CAPSULE_SHAPE_PROXYTYPE))
            {
                return capsuleBoxSweepFunc;
            }

            // failed to find an algorithm
            return emptySweepFunc;
        }
    }
}