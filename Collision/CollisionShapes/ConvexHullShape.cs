using MobaGame.FixedMath;
using System.Collections.Generic;
using System;

namespace MobaGame.Collision
{
    public class ConvexHullShape: PolyhedralConvexShape
    {
        private List<VInt3> points = new List<VInt3>();

        public ConvexHullShape(List<VInt3> points)
        {
            for(int i = 0; i < points.Count; i++)
            {
                this.points.Add(points[i]);
            }

            recalcLocalAabb();
        }

        public override void setLocalScaling(VInt3 scaling)
        {
            localScaling = scaling;
            recalcLocalAabb();
        }

        public void addPoint(VInt3 point)
        {
            points.Add(point);
            recalcLocalAabb();
        }

        public List<VInt3> getPoints()
        {
            return points;
        }

        public int getNumPoints()
        {
            return points.Count;
        }

        public override VInt3 localGetSupportingVertexWithoutMargin(VInt3 vec0)
        {
            VInt3 supVec = VInt3.zero;
            VFixedPoint newDot = VFixedPoint.MinValue;
            VFixedPoint maxDot = VFixedPoint.MinValue;

            VInt3 vec = vec0;
            VFixedPoint lenSqr = vec.sqrMagnitude;
            if (lenSqr < Globals.EPS)
            {
                vec = new VInt3(VFixedPoint.One, VFixedPoint.Zero, VFixedPoint.Zero);
            }
            else
            {
                vec = vec.Normalize();
            }

		    for (int i = 0; i<points.Count; i++) {
			    VInt3 vtx = points[i] * localScaling;

			    newDot = VInt3.Dot(vec, vtx);
			    if (newDot > maxDot) {
				    maxDot = newDot;
				    supVec = vtx;
			    }
            }
		    return supVec;
	    }

        public override VInt3 localGetSupportingVertex(VInt3 vec)
        {
            VInt3 supVertex = localGetSupportingVertexWithoutMargin(vec);

            if (getMargin() != VFixedPoint.Zero)
            {
                VInt3 vecnorm = vec;
                if (vecnorm.sqrMagnitude < Globals.EPS2)
                {
                    vecnorm = -VInt3.one;
                }
                vecnorm = vecnorm.Normalize();
                supVertex = vecnorm * getMargin() + supVertex;
            }
            return supVertex;
        }

        public override int getNumVertices()
        {
            return points.Count;
        }

        public override VInt3 getVertex(int i)
        {
            return points[i] * localScaling;
        }

        public override BroadphaseNativeType getShapeType()
        {
            return BroadphaseNativeType.CONVEX_HULL_SHAPE_PROXYTYPE;
        }
    }
}
