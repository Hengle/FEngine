﻿using MobaGame.FixedMath;
using System.Collections.Generic;
using System;

namespace MobaGame.Collision
{
    class KinematicCharacterController : ActionInterface
    {
        VInt3[] upAxisDirection = new VInt3[] { VInt3.right, VInt3.up, VInt3.forward };
        protected CollisionObject me;

        protected VFixedPoint verticalVelocity;
        protected VFixedPoint verticalOffset;
        protected VFixedPoint fallSpeed;
        protected VFixedPoint jumpSpeed;
        protected VFixedPoint maxSlopeCosine;
        protected VFixedPoint gravity;
        protected VFixedPoint stepHeight;
        protected VFixedPoint addedMargin;
        protected VInt3 walkDirection;
        protected VInt3 normalizedDirection;
        protected VInt3 currentPosition;
        protected VFixedPoint currentStepOffset;
        protected VInt3 targetPosition;

        List<ManifoldResult> manifolds = new List<ManifoldResult>();

        protected VInt3 touchingNormal;

        protected bool wasOnGround;

        protected VFixedPoint velocityTimeInterval;
        protected int upAxis;

        public KinematicCharacterController(CollisionObject me, VFixedPoint stepHeight): this(me, stepHeight, 1)
        {
            
        }

        public KinematicCharacterController(CollisionObject me, VFixedPoint stepHeight, int upAxis)
        {
            this.upAxis = upAxis;
            this.addedMargin = VFixedPoint.Two / VFixedPoint.Create(100);
            this.me = me;
            this.gravity = VFixedPoint.Create(10); // 1G acceleration
            this.fallSpeed = VFixedPoint.Create(55); // Terminal velocity of a sky diver in m/s.
            this.jumpSpeed = VFixedPoint.Create(10); 
            this.wasOnGround = false;
            VFixedPoint maxSlopeRadians = VFixedPoint.Create(50) / VFixedPoint.Create(180) / FMath.Trig.Rag2Deg;
            maxSlopeCosine = FMath.Trig.Cos(maxSlopeRadians);
        }

        public override void updateAction(CollisionWorld collisionWorld, VFixedPoint deltaTimeStep)
        {
            preStep(collisionWorld);
            playerStep(collisionWorld, deltaTimeStep);
        }

        public void setVelocityForTimeInterval(VInt3 velocity, VFixedPoint timeInterval)
        {
            walkDirection = velocity;
            normalizedDirection = walkDirection.Normalize();
		    velocityTimeInterval = timeInterval;
	    }

        public void warp(VInt3 origin)
        {
            VIntTransform transform = me.getWorldTransform();
            transform.position = origin;
            me.setWorldTransform(transform);
        }

        public void preStep(CollisionWorld collisionWorld)
        {
            int numPenetrationLoops = 0;
            while(recoverFromPenetration(collisionWorld))
            {
                numPenetrationLoops++;
                if(numPenetrationLoops > 4)
                {
                    break;
                }
            }
            currentPosition = me.getWorldTransform().position;
            targetPosition = currentPosition;
        }

        public void playerStep(CollisionWorld collisionWorld, VFixedPoint dt)
        {
            wasOnGround = onGround();

            verticalVelocity -= gravity * dt;
            if(verticalVelocity > VFixedPoint.Zero && verticalVelocity > jumpSpeed)
            {
                verticalVelocity = jumpSpeed;
            }

            if(verticalVelocity < VFixedPoint.Zero && verticalVelocity.Abs() > fallSpeed.Abs())
            {
                verticalVelocity = -fallSpeed.Abs();
            }
            verticalOffset = verticalVelocity * dt;

            VIntTransform transform = me.getWorldTransform();
            //climb up slope
            stepUp(collisionWorld);

            VFixedPoint dtMoving = FMath.Min(dt, velocityTimeInterval);
            velocityTimeInterval -= dt;
            VInt3 move = walkDirection * dtMoving;
            stepForwardAndStrafe(collisionWorld, move);
            //slide down slope
            stepDown(collisionWorld, dt);

            transform.position = currentPosition;
            me.setWorldTransform(transform);
        }

        public bool canJump()
        {
            return onGround();
        }

        public void jump()
        {
            if (!canJump()) return;

            verticalVelocity = jumpSpeed;
        }

        protected VInt3 computeReflectionDirection(VInt3 direction, VInt3 normal)
        {
            VInt3 result = normal * VInt3.Dot(direction, normal) * -2 + direction;
            return result;
        }

        protected VInt3 parallelComponent(VInt3 direction, VInt3 normal)
        {
            return normal * VInt3.Dot(direction, normal);
        }

        protected VInt3 perpendicularComponent(VInt3 direction, VInt3 normal)
        {
            VInt3 perpendicular = direction - parallelComponent(direction, normal);
            return perpendicular;
        }

        protected bool recoverFromPenetration(CollisionWorld collisionWorld)
        {
            bool penetration = false;
            VFixedPoint maxPen = VFixedPoint.Zero;

            manifolds.Clear();
            collisionWorld.OverlapTest(me, manifolds);
            for(int i = 0; i < manifolds.Count; i++)
            {
                ManifoldResult aresult = manifolds[i];
                int directionSign = aresult.body0 == me ? -1 : 1;
                VFixedPoint pen = aresult.depth;
                if(pen < VFixedPoint.Zero)
                {
                    if(pen < maxPen)
                    {
                        maxPen = pen;
                        touchingNormal = aresult.normalWorldOnB * directionSign;
                    }

                    currentPosition += aresult.normalWorldOnB * directionSign * pen * VFixedPoint.Create(0.2f);
                    penetration = true;
                }
            }

            return penetration;
        }

        protected void stepUp(CollisionWorld collisionWorld)
        {
            VIntTransform start = VIntTransform.Identity, end = VIntTransform.Identity;

            targetPosition = currentPosition + upAxisDirection[upAxis] * (stepHeight + (verticalOffset > VFixedPoint.Zero ? verticalOffset : VFixedPoint.Zero));
            start.position = currentPosition + upAxisDirection[upAxis] * (me.getCollisionShape().getMargin() + addedMargin);
            end.position = targetPosition;

            VInt3 up = -upAxisDirection[upAxis];
            KinematicClosestNotMeConvexResultCallback callback = new KinematicClosestNotMeConvexResultCallback(ghostObject, up, 0.0f);
            callback.collisionFilterGroup = me.getBroadphaseHandle().collisionFilterGroup;
            callback.collisionFilterMask = me.getBroadphaseHandle().collisionFilterMask;

            world.convexSweepTest(me, start, end, callback);

            if (callback.hasHit())
            {
                // we moved up only a fraction of the step height
                currentStepOffset = stepHeight * callback.closestHitFraction;
                currentPosition = currentPosition * callback.closestHitFraction + targetPosition * (VFixedPoint.One - callback.closestHitFraction);
                verticalVelocity = VFixedPoint.Zero;
                verticalOffset = VFixedPoint.Zero;
            }
            else {
                currentStepOffset = stepHeight;
                currentPosition = targetPosition;
            }
        }

        protected void updateTargetPositionBasedOnCollision(VInt3 hitNormal)
        {
            VInt3 movementDirection = targetPosition - currentPosition;
            VFixedPoint movementLength = movementDirection.magnitude;

            if (movementLength > Globals.EPS)
            {
                movementDirection = movementDirection.Normalize();

                VInt3 reflectionDir = computeReflectionDirection(movementDirection, hitNormal);
                reflectionDir = reflectionDir.Normalize();

                VInt3 perpendicularDir = perpendicularComponent(reflectionDir, hitNormal);
                targetPosition = currentPosition + perpendicularDir * movementLength;
            }
        }


        protected void stepForwardAndStrafe(CollisionWorld collisionWorld, VInt3 walkMove)
        {
            VIntTransform start = VIntTransform.Identity, end = VIntTransform.Identity;
            targetPosition = currentPosition + walkMove;

            VFixedPoint fraction = VFixedPoint.One;

            int maxIter = 10;
            while(fraction > VFixedPoint.Create(0.01f) && maxIter-- > 0)
            {
                start.position = currentPosition;
                end.position = targetPosition;

                KinematicClosestNotMeConvexResultCallback callback = new KinematicClosestNotMeConvexResultCallback(ghostObject, upAxisDirection[upAxis], -1.0f);
                callback.collisionFilterGroup = me.getBroadphaseHandle().collisionFilterGroup;
                callback.collisionFilterMask = me.getBroadphaseHandle().collisionFilterMask;

                VFixedPoint margin = me.getCollisionShape().getMargin();
                me.getCollisionShape().setMargin(margin + addedMargin);

                collisionWorld.convexSweepTest(me, start, end, callback);
                me.getCollisionShape().setMargin(margin);

                fraction -= callback.closestHitFraction;

                if (callback.hasHit())
                {
                    updateTargetPositionBasedOnCollision(callback.hitNormalWorld);
                    VInt3 currentDir = targetPosition - currentPosition;
                    VFixedPoint distance2 = currentDir.sqrMagnitude;
                    if(distance2 > Globals.EPS2)
                    {
                        currentDir = currentDir.Normalize();
                        if(VInt3.Dot(currentDir, normalizedDirection) <= VFixedPoint.Zero)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    currentPosition = targetPosition;
                }
            }
        }

        protected void stepDown(CollisionWorld collisionWorld, VFixedPoint dt)
        {
            VIntTransform start = VIntTransform.Identity, end = VIntTransform.Identity;

            VFixedPoint additionalDownStep = wasOnGround ? stepHeight : VFixedPoint.Zero;
            VInt3 stepDrop = upAxisDirection[upAxis] * (currentStepOffset + additionalDownStep);
            VFixedPoint downVelocity = (additionalDownStep == VFixedPoint.Zero && verticalVelocity < VFixedPoint.Zero ? -verticalVelocity : VFixedPoint.Zero) * dt;
            VInt3 gravityDrop = upAxisDirection[upAxis] * downVelocity;
            targetPosition -= stepDrop;
            targetPosition -= gravityDrop;

            start.position = currentPosition; end.position = targetPosition;

            KinematicClosestNotMeConvexResultCallback callback = new KinematicClosestNotMeConvexResultCallback(ghostObject, upAxisDirection[upAxis], maxSlopeCosine);
            callback.collisionFilterGroup = me.getBroadphaseHandle().collisionFilterGroup;
            callback.collisionFilterMask = me.getBroadphaseHandle().collisionFilterMask;

            collisionWorld.convexSweepTest(convexShape, start, end, callback);

            if (callback.hasHit())
            {
                // we dropped a fraction of the height -> hit floor
                currentPosition = currentPosition* callback.closestHitFraction + targetPosition * (VFixedPoint.One - callback.closestHitFraction);
                verticalVelocity = VFixedPoint.Zero;
                verticalOffset = VFixedPoint.Zero;
            }
            else {
                // we dropped the full height
                currentPosition = targetPosition;
            }
        }

        public bool onGround()
        {
            return verticalVelocity == VFixedPoint.Zero && verticalOffset == VFixedPoint.Zero;
        }
    }

    class KinematicClosestNotMeConvexResultCallback: CastResult
    {

        protected CollisionObject me;
        protected VInt3 up;
		protected VFixedPoint minSlopeDot;

        public KinematicClosestNotMeConvexResultCallback(CollisionObject me, VInt3 up, VFixedPoint minSlopeDot)
        {
            this.me = me;
            this.up = up;
            this.minSlopeDot = minSlopeDot;
        }

        public override void addSingleResult(CastResult convexResult)
        {
       
		}
	}
}
