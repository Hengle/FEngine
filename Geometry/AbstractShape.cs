using MobaGame.FixedMath;
using System.Text;

namespace MobaGame.Collision
{
    public abstract class AbstractShape : Shape, Transformable
    {
        public readonly UUID id = UUID.GetNextUUID();
        public VInt3 center;
        public VFixedPoint radius;
       
        public AbstractShape(VInt3 center, VFixedPoint radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Id=").Append(this.id)
              .Append("|Center=").Append(this.center)
              .Append("|Radius=").Append(this.radius);
            return sb.ToString();
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public void translate(VFixedPoint x, VFixedPoint y, VFixedPoint z)
        {
            center += new VInt3(x, y, z);
        }

        public void translate(VInt3 paramVector2)
        {
            center += paramVector2;
        }

        public abstract void rotate(VFixedPoint paramDouble, VInt3 paramVector2);

        public abstract void rotate(VFixedPoint paramDouble1, VFixedPoint paramDouble2, VFixedPoint paramDouble3);

        public UUID getId()
        {
            return id;
        }

        public VInt3 getCenter()
        {
            return center;
        }

        public VFixedPoint getRadius()
        {
            return radius;
        }

        public Interval project(VInt3 paramVector2)
        {
            return project(paramVector2, VIntTransform.Identity);
        }

        public abstract Interval project(VInt3 paramVector2, VIntTransform paramTransform);

        public bool contains(VInt3 paramVector2)
        {
            return contains(paramVector2, VIntTransform.Identity);
        }

        public abstract bool contains(VInt3 paramVector2, VIntTransform paramTransform);

        public abstract Mass createMass(VFixedPoint paramDouble);

        public AABB createAABB()
        {
            return createAABB(VIntTransform.Identity);
        }

        public abstract AABB createAABB(VIntTransform paramTransform);
    }
}
