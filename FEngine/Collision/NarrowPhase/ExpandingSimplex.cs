using MobaGame.FixedMath;
using System.Collections.Generic;
using System;

namespace MobaGame.Collision
{
    class ExpandingSimplex
    {
    	Heap<ExpandingSimplexFacet> heap;

    	public ExpandingSimplex(MinkowskiSumPoint[] simplex)
    	{
    		heap = new Heap<ExpandingSimplexFacet>();
    		ExpandingSimplexFacet[] init = new ExpandingSimplexFacet[4];
    		for(int i = 0; i < 4; i++)
    		{
    			ExpandingSimplexFacet afacet = new ExpandingSimplexFacet(simplex[i].point, simplex[(i+1)%4].point, simplex[(i+2)%4].point);
    			init[i] = afacet;
    			heap.Push(afacet);
    		}
    	}

    	public ExpandingSimplexFacet getClosestFacet()
    	{
    		return heap.pop();
    	}

    	public void expand(VInt3 point)
    	{
    		List<ExpandingSimplexFacet> visibleFacets = new List<ExpandingSimplexFacet>();
    		for(int i = 0; i < heap.size(); i++)
    		{
    			ExpandingSimplexFacet afacet = heap[i];
    			if(VInt3.Dot(point, afacet.normal) > 0)
    			{
    				visibleFacets.Add(afacet);
    				afacet.obsolete = true;
    			}
    		}

    		List<ExpandingSimplexFacetEdge> boundaryEdges = new List<ExpandingSimplexFacetEdge>();
    		for(int i = 0; i < visibleFacets.Count; i++)
    		{
    			ExpandingSimplexFacet afacet = visibleFacets[i];
    			for(int j = 0; j < 3; j++)
    			{
    				ExpandingSimplexFacetEdge aedge = afacet.Edges[i];
    				bool alreadyHasEdge = false;
    				for(int k = 0; k < boundaryEdges.Count; k++)
    				{
    					ExpandingSimplexFacetEdge earlyEdge = boundaryEdges[k];
    					if((earlyEdge.Point1 == aedge.Point1 && earlyEdge.Point2 == aedge.Point2) || 
    						(earlyEdge.Point2 == aedge.Point1 && earlyEdge.Point1 == aedge.Point2))
    					{
    						boundaryEdges.RemoveAt(k);
    						alreadyHasEdge = true;
    						break;
    					}
    				}
    				if(!alreadyHasEdge)
    				{
    					boundaryEdges.Add(aedge.AdjancentEdge);
    				}
    			}
    		}

    		for(int i = 0; i < boundaryEdges.Count; i++)
    		{
    			ExpandingSimplexFacetEdge aedge = boundaryEdges[i];
    			VInt3 edgePoint1 = aedge.Point1Index;
    			VInt3 edgePoint2 = aedge.Point2Index;
    			ExpandingSimplexFacet newFacet = new ExpandingSimplexFacet(edgePoint1, edgePoint2, point);    			
    			heap.Push(newFacet);
    		}
    	}
    }

}
