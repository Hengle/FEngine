using MobaGame.FixedMath;
using System.Text;
using System;

namespace MobaGame.Collision
{
    public class Capsule : AbstractShape, Convex, Shape, Transformable
    {
        public readonly VFixedPoint length;
        public readonly VFixedPoint capRadius;
        public readonly VInt3[] foci;
        public readonly VInt3 localXAxis;

        public Capsule(VFixedPoint length, VFixedPoint radius, VInt3 mainAxis, VInt3 center): base(center, length / VFixedPoint.Two + radius)
        {
            this.length = length;
            this.capRadius = radius;
            localXAxis = mainAxis;
            foci = new VInt3[] { mainAxis * length / VFixedPoint.Two, -mainAxis * length / VFixedPoint.Two };
        }

        public override bool contains(VInt3 paramVector2, VIntTransform paramTransform)
        {
            VInt3 closest = Segment.getPointOnSegmentClosestToPoint(paramVector2, paramTransform.TransformPoint(foci[0]), paramTransform.TransformPoint(foci[1]));
            return (paramVector2 - closest).sqrMagnitude < radius * radius;
        }

        public override AABB createAABB(VIntTransform paramTransform)
        {
            VInt3 max = new VInt3(FMath.Max(foci[0].x, foci[1].x), FMath.Max(foci[0].y, foci[1].y), FMath.Max(foci[0].z, foci[1].z));
            VInt3 min = new VInt3(FMath.Min(foci[0].x, foci[1].x), FMath.Min(foci[0].y, foci[1].y), FMath.Min(foci[0].z, foci[1].z));
            AABB aAABB = new AABB(min, max);
            aAABB.expand(radius);
            return aAABB;
        }

        public override Mass createMass(VFixedPoint paramDouble)
        {
            throw new NotImplementedException();
        }

        public VInt3[] getAxes(VInt3[] paramArrayOfVector2, VIntTransform paramTransform)
        {
            throw new NotImplementedException();
        }

        public VInt3 getFarthestPoint(VInt3 paramVector2, VIntTransform paramTransform)
        {
            VInt3 vectorNormalized = paramVector2.Normalize();
            VInt3 worldAxisVector = paramTransform.TransformVector(localXAxis/VFixedPoint.Two);
            VFixedPoint distance = VInt3.Dot(worldAxisVector, vectorNormalized).Abs() + radius;
            return paramTransform.TransformPoint(center) + vectorNormalized * distance;
        }

        public VInt3[] getFoci(VIntTransform paramTransform)
        {
            return new VInt3[] { paramTransform.TransformPoint(foci[0]), paramTransform.TransformPoint(foci[1]) };
        }

        public override Interval project(VInt3 paramVector2, VIntTransform paramTransform)
        {
            VInt3 vectorNormalized = paramVector2.Normalize();
            VFixedPoint centerCoord = VInt3.Dot(center, paramVector2);
            VInt3 worldAxisVector = paramTransform.TransformVector(localXAxis);
            VFixedPoint distance = VInt3.Dot(worldAxisVector, vectorNormalized).Abs() / 2 + radius;
            return new Interval(centerCoord - distance, centerCoord + distance);
        }

        public override void rotate(VFixedPoint paramDouble, VInt3 paramVector2)
        {
            throw new NotImplementedException();
        }

        public override void rotate(VFixedPoint paramDouble1, VFixedPoint paramDouble2, VFixedPoint paramDouble3)
        {
            throw new NotImplementedException();
        }
    }
}
