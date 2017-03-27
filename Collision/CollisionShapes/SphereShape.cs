﻿using System;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class SphereShape: CollisionShape 
    {
        VFixedPoint radius;
        VFixedPoint margin;

        public SphereShape(VFixedPoint radius) {
            this.radius = radius;
        }

        public override void getAabb(VIntTransform t, out VInt3 aabbMin, out VInt3 aabbMax)
        {
            VInt3 center = t.position;
            VFixedPoint radiusWithMargin = radius + margin + getMargin();
            VInt3 extent = new VInt3(radiusWithMargin, radiusWithMargin, radiusWithMargin);
            aabbMin = center - extent;
            aabbMax = center + extent;
        }

        public override BroadphaseNativeType getShapeType() {
            return BroadphaseNativeType.SPHERE_SHAPE_PROXYTYPE;
        }

        public VFixedPoint getRadius()
        {
            return radius + margin;
        }

        public override void setMargin(VFixedPoint margin)
        {
            this.margin = margin;
        }

        public override VFixedPoint getMargin()
        {
            return margin;
        }

        public override VInt3 support(VInt3 dir)
        {
            return dir * radius; 
        }
    }
}