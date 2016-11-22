using MobaGame.FixedMath;
using MobaGame.Framework;
using System;

namespace MobaGame.Collision
{
    public class GjkEpaSolver
    {
        const int MaxFacets = 64;
        const int MaxSupportPoints = 64;

        Heap<Facet> heap;
        VInt3[] aBuf = new VInt3[MaxSupportPoints];
        VInt3[] bBuf = new VInt3[MaxSupportPoints];
        Facet[] facetBuf = new Facet[MaxFacets];
        EdgeBuffer edgeBuffer;
        DeferredIDPoolBase facetManager = new DeferredIDPoolBase(MaxFacets);

        public int PenetrationDepth(ConvexShape a, ConvexShape b, VIntTransform transformA, VIntTransform transformB, VInt3[] Q, VInt3[] A, VInt3[] B, int size, ref VInt3 pa, ref VInt3 pb, ref VInt3 normal, ref VFixedPoint penDepth)
        {
            VFixedPoint upper_bound = VFixedPoint.MaxValue;
            VFixedPoint lower_bound = VFixedPoint.MinValue;

            aBuf[0] = A[0]; aBuf[1] = A[1]; aBuf[2] = A[2]; aBuf[3] = A[3];
            bBuf[0] = B[0]; bBuf[1] = B[1]; bBuf[2] = B[2]; bBuf[3] = B[3];

            int numVertsLocal = 0;
            heap.Clear();

            switch (size)
            {
                case 1:
                    if (!expandPoint(a, b, transformA, transformB, ref numVertsLocal, lower_bound, upper_bound))
                        return 4;
                    break;
                case 2:
                    if (!expandSegment(a, b, transformA, transformB, ref numVertsLocal, lower_bound, upper_bound))
                        return 4;
                    break;
                case 3:
                    if (!expandTriangle(ref numVertsLocal, lower_bound, upper_bound))
                        return 4;
                    break;
                case 4:
                    //check for input face normal. All face normals in this tetrahedron should be all pointing either inwards or outwards. If all face normals are pointing outward, we are good to go. Otherwise, we need to 
                    //shuffle the input vertexes and make sure all face normals are pointing outward
                    VFixedPoint planeDist0 = calculatePlaneDist(0, 1, 2, aBuf, bBuf);
                    if (planeDist0 < VFixedPoint.Zero)
                    {
                        //shuffle the input vertexes
                        VInt3 tempA = aBuf[2];
                        VInt3 tempB = bBuf[2];
                        aBuf[2] = aBuf[1];
                        bBuf[2] = bBuf[1];
                        aBuf[1] = tempA;
                        bBuf[1] = tempB;
                    }

                    Facet f0 = addFacet(0, 1, 2, lower_bound, upper_bound);
                    Facet f1 = addFacet(0, 3, 1, lower_bound, upper_bound);
                    Facet f2 = addFacet(0, 2, 3, lower_bound, upper_bound);
                    Facet f3 = addFacet(1, 3, 2, lower_bound, upper_bound);

                    if ((f0 == null) || (f1 == null) || (f2 == null) || (f3 == null) || heap.IsEmpty())
                        return 4;

                    f0.link(0, f1, 2);
                    f0.link(1, f3, 2);
                    f0.link(2, f2, 0);
                    f1.link(0, f2, 2);
                    f1.link(1, f3, 0);
                    f2.link(1, f3, 1);
                    numVertsLocal = 4;

                    break;
            }

            Facet facet = null;
            Facet bestFacet = null;

            bool hasMoreFacets = false;
            VInt3 tempa = new VInt3();
            VInt3 tempb = new VInt3();
            VInt3 q = new VInt3();

            do
            {
                facetManager.processDeferredIDs();
                facet = heap.pop(); //get the shortest distance triangle of origin from the list
                facet.m_inHeap = false;

                if (!facet.isObsolete())
                {
                    bestFacet = facet;
                    VInt3 planeNormal = facet.getPlaneNormal();
                    VFixedPoint planeDist = facet.getPlaneDist();

                    doSupport(a, b, transformA, transformB, -planeNormal, ref tempa, ref tempb, ref q);

                    VFixedPoint dist = VInt3.Dot(q, planeNormal);
                    upper_bound = dist >= VFixedPoint.Zero ? FMath.Min(upper_bound, dist) : upper_bound;
                    lower_bound = planeDist;

                    bool con0 = (upper_bound - lower_bound) <= Globals.EPS2;
                    if(con0)
                    {
                        calculateContactInformation(aBuf, bBuf, facet, a, b, ref pa, ref pb, ref normal, ref penDepth);
                        return 6;
                    }

                    VFixedPoint dif = dist - planeDist;
                    bool degeneratedCondition = dif < Globals.EPS;

                    if(degeneratedCondition)
                    {
                        calculateContactInformation(aBuf, bBuf, facet, a, b, ref pa, ref pb, ref normal, ref penDepth);
                        return 5;
                    }

                    aBuf[numVertsLocal] = tempa;
                    bBuf[numVertsLocal] = tempb;

                    int index = numVertsLocal++;
                    edgeBuffer.MakeEmpty();

                    facet.silhouette(q, aBuf, bBuf, edgeBuffer, facetManager);
                    Edge edge = edgeBuffer.Get(0);
                    int bufferSize = edgeBuffer.Size();

                    if(bufferSize > facetManager.getNumRemainingIDs())
                    {
                        calculateContactInformation(aBuf, bBuf, facet, a, b, ref pa, ref pb, ref normal, ref penDepth);
                        return 5;
                    }

                    Facet firstFacet = addFacet(edge.getTarget(), edge.getSource(), index, lower_bound, upper_bound);
                    firstFacet.link(0, edge.getFacet(), edge.getIndex());
                    Facet lastFacet = firstFacet;

                    bool degenerate = false;
                    for (int i = 1; (i < bufferSize) && (!degenerate); ++i)
                    {
                        edge = edgeBuffer.Get(i);
                        Facet newFacet = addFacet(edge.getTarget(), edge.getSource(), index, lower_bound, upper_bound);
                        bool b0 = newFacet.link(0, edge.getFacet(), edge.getIndex());
                        bool b1 = newFacet.link(2, lastFacet, 1);
                        degenerate = degenerate || !b0 || !b1;
                        lastFacet = newFacet;
                    }

                    if (degenerate)
                    {
                        calculateContactInformation(aBuf, bBuf, facet, a, b, ref pa, ref pb, ref normal, ref penDepth);
                        return 5;
                    }
                    firstFacet.link(2, lastFacet, 1);
                }
                hasMoreFacets = (heap.size() > 0);
                //after polytope expansion, we don't have a better facet to work with so that we should process the best facet
                //this sometime won't produce the MTD but the result seems close enough so we accept it as contact.
                //test case in BenchMark_GJKEPABoxConvex
                if (hasMoreFacets && heap.peak().getPlaneDist() > upper_bound)
                {
                    calculateContactInformation(aBuf, bBuf, bestFacet, a, b, ref pa, ref pb, ref normal, ref penDepth);
                    return 6;
                }

            } while (hasMoreFacets && numVertsLocal != MaxSupportPoints);

            calculateContactInformation(aBuf, bBuf, facet, a, b, ref pa, ref pb, ref normal, ref penDepth);
            return 5;
        }

        void calculateContactInformation(VInt3[] aBuf, VInt3[] bBuf, Facet facet, ConvexShape a, ConvexShape b, ref VInt3 pa, ref VInt3 pb, ref VInt3 normal, ref VFixedPoint penDepth)
        {
            VInt3 _pa = new VInt3();
            VInt3 _pb = new VInt3();
            facet.getClosestPoint(aBuf, bBuf, ref _pa, ref _pb);
            //dist > 0 means two shapes are penetrated. If dist < 0(when origin isn't inside the polytope), two shapes status are unknown
            VFixedPoint dist = facet.getPlaneDist();
            //planeNormal is pointing from B to A, however, the normal we are expecting is from A to B which match the GJK margin intersect case, therefore,
            //we need to flip the normal
            VInt3 n = -facet.getPlaneNormal();
            pa = _pa;
            pb = _pb;
            normal = n;
            penDepth = -dist;
        }

        static VFixedPoint calculatePlaneDist(int i0, int i1, int i2, VInt3[] aBuf, VInt3[] bBuf)
        {
            VInt3 pa0 = aBuf[i0];
            VInt3 pa1 = aBuf[i1];
            VInt3 pa2 = aBuf[i2];

            VInt3 pb0 = bBuf[i0];
            VInt3 pb1 = bBuf[i1];
            VInt3 pb2 = bBuf[i2];

            VInt3 p0 = pa0 - pb0;
            VInt3 p1 = pa1 - pb1;
            VInt3 p2 = pa2 - pb2;
            VInt3 v1 = p1 - p0;
            VInt3 v2 = p2 - p0;
            VInt3 v3 = p2 - p1;

            VFixedPoint v1v1 = v1.sqrMagnitude;
            VFixedPoint v2v2 = v2.sqrMagnitude;

            VInt3 v = v1v1 > v2v2 ? v2 : v1;

            VInt3 planeNormal = VInt3.Cross(v, v3).Normalize();
            return VInt3.Dot(planeNormal, p0);
        }

        bool expandPoint(ConvexShape a, ConvexShape b, VIntTransform transformA, VIntTransform transformB, ref int numVerts, VFixedPoint lowerBound, VFixedPoint upperBound)
	    {
            VInt3 x = VInt3.right;

            VInt3 q0 = new VInt3();
            doSupport(a, b, transformA, transformB, x, ref aBuf[1], ref bBuf[1], ref q0);
		    return expandSegment(a, b, transformA, transformB, ref numVerts, lowerBound, upperBound);
        }

        bool expandSegment(ConvexShape a, ConvexShape b, VIntTransform transformA, VIntTransform transformB, ref int numVerts, VFixedPoint lowerBound, VFixedPoint upperBound)
	    {
            VInt3 q3 = aBuf[0] - bBuf[0];
            VInt3 q4 = aBuf[1] - bBuf[1];

            VInt3 dir = (q3 - q4).Normalize();

            VFixedPoint sDir = VFixedPoint.One;
            VInt3 temp2 = new VInt3(sDir, sDir, sDir);
            VInt3 t1 = VInt3.Cross(temp2, dir).Normalize();
            VInt3 aux1 = VInt3.Cross(dir, t1);

            VInt3 q0 = new VInt3();
            doSupport(a, b, transformA, transformB, aux1, ref aBuf[0], ref bBuf[0], ref q0);

            VIntQuaternion qua0 = VIntQuaternion.AngleAxis(FMath.Pi * 2 / 3, dir);
            VInt3 aux2 = (qua0 * aux1).Normalize();

            VInt3 q1 = new VInt3();
            doSupport(a, b, transformA, transformB, aux2, ref aBuf[1], ref bBuf[1], ref q1);

            VInt3 aux3 = (qua0 * aux2).Normalize();//(aux2 * qua0);

            VInt3 q2 = new VInt3();
            doSupport(a, b, transformA, transformB, aux3, ref aBuf[2], ref bBuf[2], ref q2);

		    return expandTriangle(ref numVerts, lowerBound, upperBound);
        }

        bool expandTriangle(ref int numVerts, VFixedPoint lowerBound, VFixedPoint upperBound)
	    {
            numVerts = 3;

		    Facet f0 = addFacet(0, 1, 2, lowerBound, upperBound);
            Facet f1 = addFacet(1, 0, 2, lowerBound, upperBound);

		    if(f0 == null || f1 == null || heap.IsEmpty())
			    return false;

		    f0.link(0, f1, 0);
            f0.link(1, f1, 2);
            f0.link(2, f1, 1);
		
		    return true;
	    }

        Facet addFacet(int i0, int i1, int i2, VFixedPoint lower2, VFixedPoint upper2)
        {
            int facetId = facetManager.getNewID();

            Facet facet = new Facet(i0, i1, i2);
            facetBuf[facetId] = facet;
            facet.m_FacetId = facetId;

            bool validTriangle = facet.isValid2(i0, i1, i2, aBuf, bBuf, lower2, upper2);

            if (validTriangle)
            {
                heap.Push(facet);
                facet.m_inHeap = true;
            }
            else
            {
                facet.m_inHeap = false;
            }
            return facet;
        }

        bool originInTetrahedron(VInt3 p1, VInt3 p2, VInt3 p3, VInt3 p4)
        {
            int a = VoronoiSimplexSolver.pointOutsideOfPlane(VInt3.zero, p1, p2, p3, p4);
            int b = VoronoiSimplexSolver.pointOutsideOfPlane(VInt3.zero, p1, p3, p4, p2);
            int c = VoronoiSimplexSolver.pointOutsideOfPlane(VInt3.zero, p1, p4, p2, p3);
            int d = VoronoiSimplexSolver.pointOutsideOfPlane(VInt3.zero, p2, p4, p3, p1);
            return a == 0 && b == 0 && c == 0 && d == 0;
        }

        static void doSupport(ConvexShape a, ConvexShape b, VIntTransform transformA, VIntTransform transformB, VInt3 dir, ref VInt3 supportA, ref VInt3 supportB, ref VInt3 support)
	    {
            VInt3 dirInA = dir * -1;
            dirInA = transformA.InverseTransformVector(dirInA);

            VInt3 dirInAInB = dirInA;
            dirInAInB = transformB.InverseTransformVector(dirInAInB);

            VInt3 pInA = new VInt3();
            VInt3 qInB = new VInt3();

            pInA = a.localGetSupportingVertexWithoutMargin(dirInA);
            qInB = b.localGetSupportingVertexWithoutMargin(dirInAInB);

            supportA = transformA.TransformPoint(pInA);
            supportB = transformB.TransformPoint(qInB);

            support = supportA - supportB;
        }

    }

    class Facet: IComparable<Facet>
    {
        public VInt3 m_planeNormal;																										//16
        public VFixedPoint m_planeDist;																													//20

        public Facet[] m_adjFacets; //the triangle adjacent to edge i in this triangle												//32
        public int[] m_adjEdges; //the edge connected with the corresponding triangle															//35
        public int[] m_indices; //the index of vertices of the triangle																			//38
        public bool m_obsolete; //a flag to denote whether the triangle are still part of the bundeary of the new polytope							//39
        public bool m_inHeap;	//a flag to indicate whether the triangle is in the heap															//40
        public int m_FacetId;

        public Facet()
        {
            m_indices = new int[3];
            m_adjFacets = new Facet[3];
            m_adjEdges = new int[3];
        }

        public Facet(int _i0, int _i1, int _i2): this()
        {
            m_obsolete = false;
            m_inHeap = false;

            m_indices[0]= _i0;
            m_indices[1]= _i1;
            m_indices[2]= _i2;

            m_adjEdges[0] = m_adjEdges[1] = m_adjEdges[2] = -1;
        }

        public void invalidate()
        {
            m_adjFacets[0] = m_adjFacets[1] = m_adjFacets[2] = null;
            m_adjEdges[0] = m_adjEdges[1] = m_adjEdges[2] = -1;
        }

        public bool Valid()
        {
            return (m_adjFacets[0] != null) & (m_adjFacets[1] != null) & (m_adjFacets[2] != null);
        }


        public bool link(int edge0, Facet facet, int edge1)
        {
            m_adjFacets[edge0] = facet;
            m_adjEdges[edge0] = edge1;
            facet.m_adjFacets[edge1] = this;
            facet.m_adjEdges[edge1] = edge0;

            return (m_indices[edge0] == facet.m_indices[(edge1 + 1) % 3]) && (m_indices[(edge0 + 1) % 3] == facet.m_indices[edge1]);
        }

        public bool isObsolete()
        {
            return m_obsolete;
        }

        public VFixedPoint getPlaneDist(VInt3 p, VInt3[] aBuf, VInt3[] bBuf)
        {
            VInt3 pa0 = aBuf[m_indices[0]];
            VInt3 pb0 = bBuf[m_indices[0]];
            VInt3 p0 = pa0 - pb0;

            return VInt3.Dot(m_planeNormal, p - p0);
        }

        public bool isValid2(int i0, int i1, int i2, VInt3[] aBuf, VInt3[] bBuf, VFixedPoint lower, VFixedPoint upper)
        {
            VInt3 pa0 = aBuf[i0];
            VInt3 pa1 = aBuf[i1];
            VInt3 pa2 = aBuf[i2];

            VInt3 pb0 = bBuf[i0];
            VInt3 pb1 = bBuf[i1];
            VInt3 pb2 = bBuf[i2];

            VInt3 p0 = pa0 - pb0;
            VInt3 p1 = pa1 - pb1;
            VInt3 p2 = pa2 - pb2;

            VInt3 v1 = p1 - p0;
            VInt3 v2 = p2 - p0;
            VInt3 v3 = p2 - p1;

            VFixedPoint v1v1 = VInt3.Dot(v1, v1);
            VFixedPoint v2v2 = VInt3.Dot(v2, v2);

            VInt3 v =v1v1 > v2v2 ? v2 : v1;
            VInt3 denormalizedNormal = VInt3.Cross(v, v3);
            VFixedPoint norValue = VInt3.Dot(denormalizedNormal, denormalizedNormal);
            bool con = norValue > Globals.EPS;
            norValue = con ? norValue : VFixedPoint.One;

            VInt3 planeNormal = denormalizedNormal * FMath.Sqrt(norValue);
            VFixedPoint planeDist =  VInt3.Dot(planeNormal, p0);

            m_planeNormal = planeNormal;
            m_planeDist = planeDist;

            return con && planeDist >= lower && upper >= planeDist;
        }

        public VFixedPoint getPlaneDist()
        {
            return m_planeDist;
        }

        //return the plane normal
        public VInt3 getPlaneNormal()
        {
            return m_planeNormal;
        }

        //calculate the closest points for a shape pair
        public void getClosestPoint(VInt3[] aBuf, VInt3[] bBuf, ref VInt3 closestA, ref VInt3 closestB)
        {
            VInt3 pa0 = aBuf[m_indices[0]];
            VInt3 pa1 = aBuf[m_indices[1]];
            VInt3 pa2 = aBuf[m_indices[2]];

            VInt3 pb0 = bBuf[m_indices[0]];
            VInt3 pb1 = bBuf[m_indices[1]];
            VInt3 pb2 = bBuf[m_indices[2]];

            VInt3 p0 = pa0 - pb0;
            VInt3 p1 = pa1 - pb1;
            VInt3 p2 = pa2 - pb2;

            VInt3 v1 = p1 - p0;
            VInt3 v2 = p2 - p0;

            VFixedPoint v1dv1 = VInt3.Dot(v1, v1);
            VFixedPoint v1dv2 = VInt3.Dot(v1, v2);
            VFixedPoint v2dv2 = VInt3.Dot(v2, v2);

            VFixedPoint p0dv1 = VInt3.Dot(p0, v1); //V3Dot(V3Sub(p0, origin), v1);
            VFixedPoint p0dv2 = VInt3.Dot(p0, v2);

            VFixedPoint det = v1dv1 * v2dv2 - v1dv2 * v1dv2;//FSub( FMul(v1dv1, v2dv2), FMul(v1dv2, v1dv2) ); // non-negative
            VFixedPoint recip = VFixedPoint.One / det;

            VFixedPoint lambda1 = (p0dv2 * v1dv2 - p0dv1 * v2dv2) * recip;
            VFixedPoint lambda2 = (p0dv1 * v1dv2 - p0dv2 * v1dv1) * recip;

            VInt3 a0 = (pa1 - pa0) * lambda1;
            VInt3 a1 = (pa2 - pa0) * lambda2;
            VInt3 b0 = (pb1 - pb0) * lambda1;
            VInt3 b1 = (pb2 - pb0) * lambda2;
            closestA = a0 + a1 + pa0;
            closestB = b0 + b1 + pb0;
        }

        //performs a flood fill over the boundary of the current polytope.
        public void silhouette(VInt3 w, VInt3[] aBuf, VInt3[] bBuf, EdgeBuffer edgeBuffer, DeferredIDPoolBase manager)
        {
            m_obsolete = true;
            for(int a = 0; a < 3; ++a)
            {
                m_adjFacets[a].silhouette(m_adjEdges[a], w, aBuf, bBuf, edgeBuffer, manager);
            }
        }

        public void silhouette(int index, VInt3 w, VInt3[] aBuf, VInt3[] bBuf, EdgeBuffer edgeBuffer, DeferredIDPoolBase manager)
        {
            Edge[] stack = new Edge[64];
            stack[0] = new Edge(this, index);
            int size = 1;
            while (size-- > 0)
            {
                Facet f = stack[size].m_facet;
                index = stack[size].m_index;

                if (!f.m_obsolete)
                {
                    VFixedPoint pointPlaneDist = f.getPlaneDist(w, aBuf, bBuf);
                    if (pointPlaneDist < VFixedPoint.Zero)
                    {
                        edgeBuffer.Insert(f, index);
                    }
                    else
                    {
                        f.m_obsolete = true; // Facet is visible from w
                        int next = (index + 1) % 3;
                        int next2 = (next + 1) % 3;
                        stack[size++] = new Edge(f.m_adjFacets[next2], f.m_adjEdges[next2]);
                        stack[size++] = new Edge(f.m_adjFacets[next], f.m_adjEdges[next]);

                        if(!f.m_inHeap)
                        {
                            manager.deferredFreeID(f.m_FacetId);
                        }
                    }
                }
            }
        }

        public int CompareTo(Facet other)
        {
            if (m_planeDist < other.m_planeDist)
            {
                return -1;
            }
            else if (m_planeDist == other.m_planeDist)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        public int this[int index]
        {
            get
            {
                return m_indices[index];
            }
        }
    }

    class Edge
    {
        public Facet m_facet;
        public int m_index;

        public Edge() {}
        public Edge(Facet facet, int index)
        {
            m_facet = facet;
            m_index = index;
        }
        public Edge(Edge other)
        {
            m_facet = other.m_facet;
            m_index = other.m_index;
        }

        public Facet getFacet()
        {
            return m_facet;
        }

        public int getIndex()
        {
            return m_index;
        }

        //get out the associated start vertex index in this edge from the facet
        public int getSource()
        {
            return m_facet[m_index];
        }

        //get out the associated end vertex index in this edge from the facet
        public int getTarget()
        {
            return m_facet[(m_index + 1) % 3];
        }


    };


    class EdgeBuffer
    {
        private const int MaxEdges = 32;
        private int m_Size;
        private Edge[] m_pEdges;

        public EdgeBuffer()
        {
            m_Size = 0;
            m_pEdges = new Edge[MaxEdges];
        }

        public Edge Insert(Edge edge)
        {
            Edge pEdge = m_pEdges[m_Size++];
            pEdge = edge;
            return pEdge;
        }

        public Edge Insert(Facet facet, int index)
        {
            Edge pEdge = m_pEdges[m_Size++];
            pEdge.m_facet=facet;
            pEdge.m_index=index;
            return pEdge;
        }

        public Edge Get(int index)
        {
            return m_pEdges[index];
        }

        public int Size()
        {
            return m_Size;
        }

        public bool IsEmpty()
        {
            return m_Size == 0;
        }

        public void MakeEmpty()
        {
            m_Size = 0;
        }
    }
}
