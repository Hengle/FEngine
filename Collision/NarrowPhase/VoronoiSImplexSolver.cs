using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class VoronoiSimplexSolver: SimplexSolverInterface
    {
        protected ObjectPool<SubSimplexClosestResult> subsimplexResultsPool = new ObjectPool<SubSimplexClosestResult>();
	
	    private static  int VORONOI_SIMPLEX_MAX_VERTS = 4;

        private static  int VERTA = 0;
        private static  int VERTB = 1;
        private static  int VERTC = 2;
        private static  int VERTD = 3;

        public int _numVertices;

        public  VInt3[] simplexVectorW = new VInt3[VORONOI_SIMPLEX_MAX_VERTS];
	    public  VInt3[] simplexPointsP = new VInt3[VORONOI_SIMPLEX_MAX_VERTS];
	    public  VInt3[] simplexPointsQ = new VInt3[VORONOI_SIMPLEX_MAX_VERTS];

	    public  VInt3 cachedP1;
        public  VInt3 cachedP2;
        public  VInt3 cachedV;

        public  SubSimplexClosestResult cachedBC = new SubSimplexClosestResult();

        public bool needsUpdate;

        public VoronoiSimplexSolver()
        {
            for (int i = 0; i < VORONOI_SIMPLEX_MAX_VERTS; i++)
            {
                simplexVectorW[i] = new VInt3();
                simplexPointsP[i] = new VInt3();
                simplexPointsQ[i] = new VInt3();
            }
        }

        public void removeVertex(int index)
        {
            _numVertices--;
            simplexVectorW[index] = simplexVectorW[_numVertices];
            simplexPointsP[index] = simplexPointsP[_numVertices];
            simplexPointsQ[index] = simplexPointsQ[_numVertices];
        }

        public void reduceVertices(UsageBitfield usedVerts)
        {
            if ((numVertices() >= 4) && (!usedVerts.usedVertexD))
                removeVertex(3);

            if ((numVertices() >= 3) && (!usedVerts.usedVertexC))
                removeVertex(2);

            if ((numVertices() >= 2) && (!usedVerts.usedVertexB))
                removeVertex(1);

            if ((numVertices() >= 1) && (!usedVerts.usedVertexA))
                removeVertex(0);
        }

        public COMPUTE_POINTS_RESULT updateClosestVectorAndPoints()
        {
            if (needsUpdate)
            {
                cachedBC.reset();

                needsUpdate = false;

                switch (numVertices())
                {
                    case 0:
                        return COMPUTE_POINTS_RESULT.NOT_CONTACT;
				    case 1:
                        {
                            cachedP1 = simplexPointsP [0];
                            cachedP2 = simplexPointsQ [0];
                            cachedV = cachedP1 - cachedP2;
                            cachedBC.reset ();
                            cachedBC.setBarycentricCoordinates (VFixedPoint.One, VFixedPoint.Zero, VFixedPoint.Zero, VFixedPoint.Zero);
                            if (cachedV == VInt3.zero)
                                return COMPUTE_POINTS_RESULT.DEGENERATED;
                            return COMPUTE_POINTS_RESULT.NOT_CONTACT;
                        }
                    case 2:
                        {
                            VInt3 tmp = new VInt3();

                            //closest point origin from line segment
                            VInt3 from = simplexVectorW[0];
                            VInt3 to = simplexVectorW[1];

                            VInt3 p = VInt3.zero;
                            VInt3 diff = p - from;

                            VInt3 v = to - from;
					        VFixedPoint t = VInt3.Dot(v, diff);

					        if (t > VFixedPoint.Zero) {
						        VFixedPoint dotVV = v.sqrMagnitude;
						        if (t<dotVV) {
							        t /= dotVV;
							        cachedBC.usedVertices.usedVertexA = true;
							        cachedBC.usedVertices.usedVertexB = true;
						        } else {
							        t = VFixedPoint.One;
							        // reduce to 1 point
							        cachedBC.usedVertices.usedVertexB = true;
						        }
					        } else
					        {
						        t = VFixedPoint.Zero;
						        //reduce to 1 point
						        cachedBC.usedVertices.usedVertexA = true;
					        }
					        cachedBC.setBarycentricCoordinates(VFixedPoint.One-t, t, VFixedPoint.Zero, VFixedPoint.Zero);


					        tmp = simplexPointsP[1] - simplexPointsP[0];
					        tmp *= t;
					        cachedP1 = simplexPointsP[0] + tmp;

					        tmp = simplexPointsQ[1] - simplexPointsQ[0];
					        tmp *= t;
					        cachedP2 = simplexPointsQ[0] + tmp;

					        cachedV = cachedP1 - cachedP2;


                            reduceVertices(cachedBC.usedVertices);

                            if(cachedV == VInt3.zero)
                                return COMPUTE_POINTS_RESULT.DEGENERATED;
                            return COMPUTE_POINTS_RESULT.NOT_CONTACT;
				        }
			        case 3: 
				        { 				
					        // closest point origin from triangle 
					        VInt3 p = VInt3.zero;

					        VInt3 a = simplexVectorW[0];
                            VInt3 b = simplexVectorW[1];
                            VInt3 c = simplexVectorW[2];
                            closestPtPointTriangle(p, a, b, c, cachedBC);

					        cachedP1 = simplexPointsP[0] * cachedBC.barycentricCoords[0] + simplexPointsP[1] * cachedBC.barycentricCoords[1] + simplexPointsP[2] * cachedBC.barycentricCoords[2];
                            cachedP2 = simplexPointsQ[0] * cachedBC.barycentricCoords[0] + simplexPointsQ[1] * cachedBC.barycentricCoords[1] + simplexPointsQ[2] * cachedBC.barycentricCoords[2];
                            cachedV = cachedP1 - cachedP2;
                            reduceVertices(cachedBC.usedVertices);
				            if(cachedV == VInt3.zero)
				                return COMPUTE_POINTS_RESULT.DEGENERATED;
				            return COMPUTE_POINTS_RESULT.NOT_CONTACT;
				        }
			        case 4:
				        {				
					        VInt3 p = VInt3.zero;

                            VInt3 a = simplexVectorW[0];
                            VInt3 b = simplexVectorW[1];
                            VInt3 c = simplexVectorW[2];
                            VInt3 d = simplexVectorW[3];

                            bool hasSeperation = closestPtPointTetrahedron(p, a, b, c, d, cachedBC);

                            if (hasSeperation)
                            {
                                cachedP1 = simplexPointsP[0] * cachedBC.barycentricCoords[0] + simplexPointsP[1] * cachedBC.barycentricCoords[1] + simplexPointsP[2] * cachedBC.barycentricCoords[2] + simplexPointsP[3] * cachedBC.barycentricCoords[3];
                                cachedP2 = simplexPointsQ[0] * cachedBC.barycentricCoords[0] + simplexPointsQ[1] * cachedBC.barycentricCoords[1] + simplexPointsQ[2] * cachedBC.barycentricCoords[2] + simplexPointsQ[3] * cachedBC.barycentricCoords[3];
                                cachedV = cachedP1 - cachedP2;
                                reduceVertices(cachedBC.usedVertices);
                                return COMPUTE_POINTS_RESULT.NOT_CONTACT;
                            }
                            else
                            {
                                if (cachedBC.degenerated)
                                    return COMPUTE_POINTS_RESULT.DEGENERATED;
                                else
                                    return COMPUTE_POINTS_RESULT.CONTACT;
                            }
				        }
			        default:
				    {
                        return COMPUTE_POINTS_RESULT.NOT_CONTACT;
				    }
			    }
		    }

		    return COMPUTE_POINTS_RESULT.NOT_CONTACT;
	    }

        public bool closestPtPointTriangle(VInt3 p, VInt3 a, VInt3 b, VInt3 c, SubSimplexClosestResult result)
        {
            result.usedVertices.reset();

            // Check if P in vertex region outside A
            VInt3 ab = b - a;
            VInt3 ac = c - a;
            VInt3 ap = p - a;

		    VFixedPoint d1 = VInt3.Dot(ab, ap);
            VFixedPoint d2 = VInt3.Dot(ac, ap);

		    if (d1 <= VFixedPoint.Zero && d2 <= VFixedPoint.Zero) 
		    {
			    result.closestPointOnSimplex = a;
			    result.usedVertices.usedVertexA = true;
			    result.setBarycentricCoordinates(VFixedPoint.One, VFixedPoint.Zero, VFixedPoint.Zero, VFixedPoint.Zero);
			    return true; // a; // barycentric coordinates (1,0,0)
		    }

            // Check if P in vertex region outside B
            VInt3 bp = p - b;

            VFixedPoint d3 = VInt3.Dot(ab, bp);
            VFixedPoint d4 = VInt3.Dot(ac, bp);

		    if (d3 >= VFixedPoint.Zero && d4 <= d3) 
		    {
			    result.closestPointOnSimplex = b;
			    result.usedVertices.usedVertexB = true;
			    result.setBarycentricCoordinates(VFixedPoint.Zero, VFixedPoint.One, VFixedPoint.Zero, VFixedPoint.Zero);

			    return true; // b; // barycentric coordinates (0,1,0)
		    }

            // Check if P in edge region of AB, if so return projection of P onto AB
            VFixedPoint vc = d1 * d4 - d3 * d2;
		    if (vc <= VFixedPoint.Zero && d1 >= VFixedPoint.Zero && d3 <= VFixedPoint.Zero) {
                VFixedPoint v1 = d1 / (d1 - d3);
                result.closestPointOnSimplex = ab * v1 + a;
			    result.usedVertices.usedVertexA = true;
			    result.usedVertices.usedVertexB = true;
			    result.setBarycentricCoordinates(VFixedPoint.One-v1, v1, VFixedPoint.Zero, VFixedPoint.Zero);
			    return true;
			    //return a + v * ab; // barycentric coordinates (1-v,v,0)
		    }

		    // Check if P in vertex region outside C
		    VInt3 cp = p - c;

		    VFixedPoint d5 = VInt3.Dot(ab, cp);
            VFixedPoint d6 = VInt3.Dot(ac, cp);

		    if (d6 >= VFixedPoint.Zero && d5 <= d6) 
		    {
			    result.closestPointOnSimplex = c;
			    result.usedVertices.usedVertexC = true;
			    result.setBarycentricCoordinates(VFixedPoint.Zero, VFixedPoint.Zero, VFixedPoint.One, VFixedPoint.Zero);
			    return true;//c; // barycentric coordinates (0,0,1)
		    }

            // Check if P in edge region of AC, if so return projection of P onto AC
            VFixedPoint vb = d5 * d2 - d1 * d6;
		    if (vb <= VFixedPoint.Zero && d2 >= VFixedPoint.Zero && d6 <= VFixedPoint.Zero)
            {
                VFixedPoint w1 = d2 / (d2 - d6);
                result.closestPointOnSimplex = ac * w1 + a;
			    result.usedVertices.usedVertexA = true;
			    result.usedVertices.usedVertexC = true;
			    result.setBarycentricCoordinates(VFixedPoint.One-w1, VFixedPoint.Zero, w1, VFixedPoint.Zero);
			    return true;
		    }

            // Check if P in edge region of BC, if so return projection of P onto BC
            VFixedPoint va = d3 * d6 - d5 * d4;
		    if (va <= VFixedPoint.Zero && (d4 - d3) >= VFixedPoint.Zero && (d5 - d6) >= VFixedPoint.Zero)
            {
                VFixedPoint w1 = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                result.closestPointOnSimplex = (c - b) * w1 + b;
			    result.usedVertices.usedVertexB = true;
			    result.usedVertices.usedVertexC = true;
			    result.setBarycentricCoordinates(VFixedPoint.Zero, VFixedPoint.One-w1, w1, VFixedPoint.Zero);
			    return true;		
		    }

		    // P inside face region. Compute Q through its barycentric coordinates (u,v,w)
		    VFixedPoint denom = VFixedPoint.One / (va + vb + vc);
            VFixedPoint v = vb * denom;
            VFixedPoint w = vc * denom;
            result.closestPointOnSimplex = ab * v + ac * w;
		    result.usedVertices.usedVertexA = true;
		    result.usedVertices.usedVertexB = true;
		    result.usedVertices.usedVertexC = true;
		    result.setBarycentricCoordinates(VFixedPoint.One-v-w, v, w, VFixedPoint.Zero);

		    return true;
	    }

        //return: -1 degenerate, 0 p is inside abcd, 1 p is outside abcd
        public static int pointOutsideOfPlane(VInt3 p, VInt3 a, VInt3 b, VInt3 c, VInt3 d)
        {
		    VInt3 normal = VInt3.Cross(b - a, c - a);
		    VFixedPoint signp = VInt3.Dot(p - a, normal); // [AP AB AC]
            VFixedPoint signd = VInt3.Dot(d - a, normal); // [AD AB AC]
		    if (signd * signd < Globals.EPS)
		    {
			    return -1;
		    }
		    return (signp * signd < VFixedPoint.Zero)? 1 : 0;
        }

        public bool closestPtPointTetrahedron(VInt3 p, VInt3 a, VInt3 b, VInt3 c, VInt3 d, SubSimplexClosestResult finalResult)
        {
            SubSimplexClosestResult tempResult = subsimplexResultsPool.Get();
            tempResult.reset();
            try
            {
                VInt3 tmp = new VInt3();
			    VInt3 q = new VInt3();

			    // Start out assuming point inside all halfspaces, so closest to itself
			    finalResult.closestPointOnSimplex = p;
			    finalResult.usedVertices.reset();
			    finalResult.usedVertices.usedVertexA = true;
			    finalResult.usedVertices.usedVertexB = true;
			    finalResult.usedVertices.usedVertexC = true;
			    finalResult.usedVertices.usedVertexD = true;

			    int pointOutsideABC = pointOutsideOfPlane(p, a, b, c, d);
                int pointOutsideACD = pointOutsideOfPlane(p, a, c, d, b);
                int pointOutsideADB = pointOutsideOfPlane(p, a, d, b, c);
                int pointOutsideBDC = pointOutsideOfPlane(p, b, d, c, a);

		        if (pointOutsideABC < 0 || pointOutsideACD < 0 || pointOutsideADB < 0 || pointOutsideBDC < 0)
		        {
                    //degenerate condition, no need to update
                    cachedBC.degenerated = true;
			        return false;
		        }

		        if (pointOutsideABC == 0 && pointOutsideACD == 0 && pointOutsideADB == 0 && pointOutsideBDC == 0)
			    {
                    //point is inside tethahedron, no need to update
				     return false;
			    }


                VFixedPoint bestSqDist = VFixedPoint.MaxValue;
			    // If point outside face abc then compute closest point on abc
			    if (pointOutsideABC != 0) 
			    {
                    closestPtPointTriangle(p, a, b, c, tempResult);
                    q = tempResult.closestPointOnSimplex;

				    tmp = q - p;
				    VFixedPoint sqDist = tmp.sqrMagnitude;
				    // Update best closest point if (squared) distance is less than current best
				    if (sqDist<bestSqDist) {
					    bestSqDist = sqDist;
					    finalResult.closestPointOnSimplex = q;
					    //convert result bitmask!
					    finalResult.usedVertices.reset();
					    finalResult.usedVertices.usedVertexA = tempResult.usedVertices.usedVertexA;
					    finalResult.usedVertices.usedVertexB = tempResult.usedVertices.usedVertexB;
					    finalResult.usedVertices.usedVertexC = tempResult.usedVertices.usedVertexC;
					    finalResult.setBarycentricCoordinates(
							    tempResult.barycentricCoords[VERTA],
							    tempResult.barycentricCoords[VERTB],
							    tempResult.barycentricCoords[VERTC],
							    VFixedPoint.Zero
					    );
				    }
			    }


			    // Repeat test for face acd
			    if (pointOutsideACD != 0) 
			    {
                    closestPtPointTriangle(p, a, c, d, tempResult);
                    q = tempResult.closestPointOnSimplex;
				    //convert result bitmask!

				    tmp = q - p;
				    VFixedPoint sqDist = tmp.sqrMagnitude;
				    if (sqDist<bestSqDist) 
				    {
					    bestSqDist = sqDist;
					    finalResult.closestPointOnSimplex = q;
					    finalResult.usedVertices.reset();
					    finalResult.usedVertices.usedVertexA = tempResult.usedVertices.usedVertexA;

					    finalResult.usedVertices.usedVertexC = tempResult.usedVertices.usedVertexB;
					    finalResult.usedVertices.usedVertexD = tempResult.usedVertices.usedVertexC;
					    finalResult.setBarycentricCoordinates(
							    tempResult.barycentricCoords[VERTA],
							    VFixedPoint.Zero,
							    tempResult.barycentricCoords[VERTB],
							    tempResult.barycentricCoords[VERTC]
					    );
				    }
			    }

			    if (pointOutsideADB != 0)
			    {
                    closestPtPointTriangle(p, a, d, b, tempResult);
                    q = tempResult.closestPointOnSimplex;
				    //convert result bitmask!

				    tmp = q - p;
				    VFixedPoint sqDist = tmp.sqrMagnitude;
				    if (sqDist<bestSqDist) 
				    {
					    bestSqDist = sqDist;
					    finalResult.closestPointOnSimplex = q;
					    finalResult.usedVertices.reset();
					    finalResult.usedVertices.usedVertexA = tempResult.usedVertices.usedVertexA;
					    finalResult.usedVertices.usedVertexB = tempResult.usedVertices.usedVertexC;

					    finalResult.usedVertices.usedVertexD = tempResult.usedVertices.usedVertexB;
					    finalResult.setBarycentricCoordinates(
							    tempResult.barycentricCoords[VERTA],
							    tempResult.barycentricCoords[VERTC],
							    VFixedPoint.Zero,
							    tempResult.barycentricCoords[VERTB]
					    );
				    }
			    }

			    // Repeat test for face bdc
			    if (pointOutsideBDC != 0)
			    {

                    closestPtPointTriangle(p, b, d, c, tempResult);
                    q = tempResult.closestPointOnSimplex;
				    //convert result bitmask!
				    tmp = q - p;
				    VFixedPoint sqDist = tmp.sqrMagnitude;
				    if (sqDist<bestSqDist) 
				    {
					    bestSqDist = sqDist;
					    finalResult.closestPointOnSimplex = q;
					    finalResult.usedVertices.reset();
					    //
					    finalResult.usedVertices.usedVertexB = tempResult.usedVertices.usedVertexA;
					    finalResult.usedVertices.usedVertexC = tempResult.usedVertices.usedVertexC;
					    finalResult.usedVertices.usedVertexD = tempResult.usedVertices.usedVertexB;

					    finalResult.setBarycentricCoordinates(
							    VFixedPoint.Zero,
							    tempResult.barycentricCoords[VERTA],
							    tempResult.barycentricCoords[VERTC],
							    tempResult.barycentricCoords[VERTB]
					    );
				    }
			    }

			    return true;
		    }
		    finally
            {
			    subsimplexResultsPool.Release(tempResult);
		    }
	    }

        /**
	     * Clear the simplex, remove all the vertices.
	     */
        public override void reset()
        {
            _numVertices = 0;
            needsUpdate = true;
            cachedBC.reset();
        }

        public override void addVertex(VInt3 w, VInt3 p, VInt3 q)
        {
            needsUpdate = true;

            simplexVectorW[_numVertices] = w;
            simplexPointsP[_numVertices] = p;
            simplexPointsQ[_numVertices] = q;

            _numVertices++;
        }

        public override VFixedPoint maxVertex()
        {
            int i, numverts = numVertices();
            VFixedPoint maxV = VFixedPoint.Zero;
            for (i = 0; i < numverts; i++)
            {
                VFixedPoint curLen2 = simplexVectorW[i].sqrMagnitude;
                if (maxV < curLen2)
                {
                    maxV = curLen2;
                }
            }
            return maxV;
        }

        public override bool fullSimplex()
        {
            return (_numVertices == 4);
        }

        public override int getSimplex(VInt3[] pBuf, VInt3[] qBuf, VInt3[] yBuf)
        {
            for (int i = 0; i < numVertices(); i++)
            {
                yBuf[i] = simplexVectorW[i];
                pBuf[i] = simplexPointsP[i];
                qBuf[i] = simplexPointsQ[i];
            }
            return numVertices();
        }

        public override bool emptySimplex()
        {
            return (numVertices() == 0);
        }

        public override COMPUTE_POINTS_RESULT compute_points(out VInt3 p1, out VInt3 p2)
        {
            COMPUTE_POINTS_RESULT result = updateClosestVectorAndPoints();
            p1 = cachedP1;
            p2 = cachedP2;
            return result;
        }

        public override int numVertices()
        {
            return _numVertices;
        }
    }

    public class UsageBitfield
    {
        public bool usedVertexA;
        public bool usedVertexB;
        public bool usedVertexC;
        public bool usedVertexD;

        public void reset()
        {
            usedVertexA = false;
            usedVertexB = false;
            usedVertexC = false;
            usedVertexD = false;
        }
    }

    public class SubSimplexClosestResult
    {
        public VInt3 closestPointOnSimplex = new VInt3();
        //MASK for m_usedVertices
        //stores the simplex vertex-usage, using the MASK, 
        // if m_usedVertices & MASK then the related vertex is used
        public UsageBitfield usedVertices = new UsageBitfield();
        public VFixedPoint[] barycentricCoords = new VFixedPoint[4];
        public bool degenerated = false;

        public void reset()
        {
            setBarycentricCoordinates(VFixedPoint.Zero, VFixedPoint.Zero, VFixedPoint.Zero, VFixedPoint.Zero);
            usedVertices.reset();
        }

        public void setBarycentricCoordinates(VFixedPoint a, VFixedPoint b, VFixedPoint c, VFixedPoint d)
        {
            barycentricCoords[0] = a;
            barycentricCoords[1] = b;
            barycentricCoords[2] = c;
            barycentricCoords[3] = d;
        }
    }
}
