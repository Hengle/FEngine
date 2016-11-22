using System;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public abstract class ConvexInternalShape: ConvexShape
    {
        protected VInt3 localScaling = new VInt3(VFixedPoint.One, VFixedPoint.One, VFixedPoint.One);
        protected VInt3 implicitShapeDimensions = new VInt3();
        protected VFixedPoint collisionMargin = Globals.CONVEX_DISTANCE_MARGIN;

        public override void getAabb(VIntTransform t, out VInt3 aabbMin, out VInt3 aabbMax)
        {
            getAabbSlow(t, out aabbMin, out aabbMax);
        }

        public override void getAabbSlow(VIntTransform trans, out VInt3 minAabb, out VInt3 maxAabb) {
            VFixedPoint margin = getMargin();

            maxAabb = minAabb = VInt3.zero;

            for (int i=0;i<3;i++)
            {
                VInt3 vec = VInt3.zero;
                vec[i] = VFixedPoint.One;

                VInt3 tmp1 = trans.InverseTransformVector(vec);
                VInt3 tmp2 = localGetSupportingVertex(tmp1);
                trans.TransformPoint(tmp2);
                maxAabb[i] = tmp2[i] + margin;

                vec[i] = -VFixedPoint.One;
                tmp1 = trans.InverseTransformVector(vec);
                tmp2 = localGetSupportingVertex(tmp1);
                trans.TransformPoint(tmp2);
                minAabb[i] = tmp2[i] - margin;
            }
        }

        public override VInt3 localGetSupportingVertex(VInt3 vec) {
            VInt3 supVertex = localGetSupportingVertexWithoutMargin(vec);

            if (getMargin() != VFixedPoint.Zero) {
                if (vec.sqrMagnitude < (Globals.EPS2)) {
                    vec = -VInt3.one;
                }
                vec = vec.Normalize();
                supVertex = vec * getMargin() + supVertex;
            }
            return supVertex;
        }

        public override void setLocalScaling(VInt3 scaling) {
            localScaling = new VInt3(scaling.x.Abs(), scaling.y.Abs(), scaling.z.Abs());
        }

        public override VInt3 getLocalScaling() {
            return localScaling;
        }

        public override VFixedPoint getMargin() {
            return collisionMargin;
        }

        public override void setMargin(VFixedPoint margin) {
            this.collisionMargin = margin;
        }
    }
}