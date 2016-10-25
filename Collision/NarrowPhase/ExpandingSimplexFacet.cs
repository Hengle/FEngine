using MobaGame.FixedMath;
using System.Collections.Generic;
using System;

namespace MobaGame.Collision
{
	class ExpandingSimplexFacet:IComparable<ExpandingSimplexFacet>
    {
        public ExpandingSimplexFacetEdge[] Edges;

        public VInt3 normal;
        public VFixedPoint PlaneDist;
        public bool obsolete;

        public ExpandingSimplexFacet(VInt3 a, VInt3 b, VInt3 c)
        {
            obsolete = false;

            Edges = new ExpandingSimplexFacetEdge[3];

			VInt3[] Points = new VInt3[]{a, b, c};
            for(int i = 0; i < 3; i++)
            {
                ExpandingSimplexFacetEdge aedge = new ExpandingSimplexFacetEdge();
                aedge.Point1 = Points[i];
                aedge.Point2 = Points[(i + 1) % 3];
                aedge.index = i;
                Edges[i] = aedge;
            }

            VInt3 ab = b - a;
            VInt3 ac = c - a;
            normal = VInt3.Cross(ab, ac);
			if(VInt3.Dot(normal, a) <= VFixedPoint.Zero)
            {
                normal = -normal;
            }
            normal = normal.Normalize();

            PlaneDist = VInt3.Dot(normal, a);

        }

		public int CompareTo(ExpandingSimplexFacet other)
        {
            if(PlaneDist < other.PlaneDist)
            {
                return -1;
            }
            else if(PlaneDist == other.PlaneDist)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }

    public class ExpandingSimplexFacetEdge
    {
        public VInt3 Point1;
        public VInt3 Point2;

        public int index;
    } 
}
