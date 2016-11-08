using MobaGame.FixedMath;
using System.Text;
using System;

namespace MobaGame.Collision
{
    public class Sphere : AbstractShape, Convex, Shape, Transformable
    {

        public Sphere(VInt3 center, VFixedPoint radius):base(center, radius)
        { }

        public override bool contains(VInt3 paramVector2, VIntTransform paramTransform)
        {
            return (paramVector2 - paramTransform.TransformPoint(center)).sqrMagnitude < radius * radius;
        }

        public override AABB createAABB(VIntTransform paramTransform)
        {
            return new AABB(paramTransform.TransformPoint(center), radius);
        }

        public override Mass createMass(VFixedPoint paramDouble)
        {
            return null;
        }

        public VInt3[] getAxes(VInt3[] paramArrayOfVector2, VIntTransform paramTransform)
        {
            return null;
        }

        public VInt3 getFarthestPoint(VInt3 paramVector2, VIntTransform paramTransform)
        {
            return paramTransform.TransformPoint(center) + paramVector2.Normalize() * radius;
        }

        public VInt3[] getFoci(VIntTransform paramTransform)
        {
            VInt3[] foci = new VInt3[] { paramTransform.TransformPoint(center) };
            return foci;
        }

        public override Interval project(VInt3 paramVector2, VIntTransform paramTransform)
        {
            VFixedPoint centerCoord = VInt3.Dot(paramTransform.TransformPoint(center), paramVector2);
            return new Interval(centerCoord - radius, centerCoord + radius);
        }

        public override void rotate(VFixedPoint paramDouble, VInt3 paramVector2)
        {
            
        }

        public override void rotate(VFixedPoint paramDouble1, VFixedPoint paramDouble2, VFixedPoint paramDouble3)
        {
            
        }
    }
}
