using MobaGame.FixedMath;
using System;

namespace MobaGame.Collision
{
    public class GjkEpaSolver
    {

    }

    class Facet: IComparable<Facet>
    {
        VInt3 m_planeNormal;																										//16
        VFixedPoint m_planeDist;																													//20

        Facet[] m_adjFacets; //the triangle adjacent to edge i in this triangle												//32
        int[] m_adjEdges; //the edge connected with the corresponding triangle															//35
        int[] m_indices; //the index of vertices of the triangle																			//38
        bool m_obsolete; //a flag to denote whether the triangle are still part of the bundeary of the new polytope							//39
        bool m_inHeap;	//a flag to indicate whether the triangle is in the heap															//40
        int m_FacetId;

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

            VFixedPoint v1v1 = V3Dot(v1, v1);
            VFixedPoint v2v2 = V3Dot(v2, v2);

            VInt3 v = V3Sel(FIsGrtr(v1v1, v2v2), v2, v1);
            VInt3 denormalizedNormal = V3Cross(v, v3);
            VFixedPoint norValue = V3Dot(denormalizedNormal, denormalizedNormal);
            bool con = norValue > VFixedPoint.One / VFixedPoint.Create(10000);
            norValue = con : norValue ? VFixedPoint.One;

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
        public VFixedPoint getPlaneNormal()
        {
            return m_planeNormal;
        }

        //calculate the closest points for a shape pair
        public void getClosestPoint(VInt3 aBuf, VInt3 bBuf, ref VInt3 closestA, ref VInt3 closestB)
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
        public void silhouette(VInt3 w, VInt3 aBuf, VInt3 bBuf, EdgeBuffer edgeBuffer, EPAFacetManager manager)
        {

        }

        public void silhouette(int index, VInt3 w, VInt3 aBuf, VInt3 bBuf, EdgeBuffer& edgeBuffer, EPAFacetManager& manager)
        {

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
    }

    class Edge
    {
        private Facet m_facet;
        private int m_index;

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
        public int getTarget() const
        {
            return m_facet[(m_index + 1) % 3];
        }


    };


    class EdgeBuffer
    {
        private const int MaxEdges = 32;
        private int m_Size;
        private Edge m_pEdges[];

        public EdgeBuffer()
        {
            m_Size = 0;
            m_pEdges = new Edge[MaxEdges];
        }

        public Edge Insert(const Edge& edge)
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
