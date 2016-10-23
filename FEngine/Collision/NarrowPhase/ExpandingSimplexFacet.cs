using MobaGame.FixedMath;
using System.Collections.Generic;
using System;

namespace MobaGame.Collision
{
    class ExpandingSimplexFacet:IComparable<ExpandingSimplexFacet>
    {
        public Edge[] Edges;
        public VInt3[] Points;
        public ExpandingSimplexFacet[] AdjancentFacets;

        public VInt3 normal;
        bool obsolete;
		bool inHeap;

        public ExpandingSimplexFacet(VInt3 a, VInt3 b, VInt3 c)
        {
            obsolete = false;
            inHeap = false;
            Edges = new Edge[3];
            Points = new VInt3[3];
            AdjancentFacets = new ExpandingSimplexFacet[3];

            VInt3 ab = b - a;
            VInt3 ac = c - a;
            VInt3 normal = VInt3.Cross(ab, ac);
            if()
        }
    }

    class Edge
    {
        public ExpandingSimplexFacet Triangle;
        public Edge AdjancentEdge;

        public int Point1Index;
        public int Point2Index;
    } 
}
