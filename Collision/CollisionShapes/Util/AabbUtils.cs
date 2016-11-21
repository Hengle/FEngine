﻿using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{ 
    public static class AabbUtils
    {
        public static bool RayAabb2(VInt3 rayFrom,
                                  VInt3 rayInvDirection,
								  uint[] raySign,
                                  VInt3[] bounds,
                                  ref VFixedPoint tmin,
								  VFixedPoint lambda_min,
                                  VFixedPoint lambda_max)
        {
            tmin = (bounds[raySign[0]].x - rayFrom.x) * rayInvDirection.x;
            VFixedPoint tmax = (bounds[1 - raySign[0]].x - rayFrom.x) * rayInvDirection.x;
            VFixedPoint tymin = (bounds[raySign[1]].y - rayFrom.y) * rayInvDirection.y;
            VFixedPoint tymax = (bounds[1 - raySign[1]].y - rayFrom.y) * rayInvDirection.y;

	        if ( (tmin > tymax) || (tymin > tmax) )
		        return false;

	        if (tymin > tmin)
		        tmin = tymin;

	        if (tymax<tmax)

                tmax = tymax;

            VFixedPoint tzmin = (bounds[raySign[2]].z - rayFrom.z) * rayInvDirection.z;
            VFixedPoint tzmax = (bounds[1 - raySign[2]].z - rayFrom.z) * rayInvDirection.z;

	        if ( (tmin > tzmax) || (tzmin > tmax) )
		        return false;
	        if (tzmin > tmin)
		        tmin = tzmin;
	        if (tzmax<tmax)

                tmax = tzmax;
	        return ( (tmin<lambda_max) && (tmax > lambda_min) );
        }

        static int btOutcode(VInt3 p, VInt3 halfExtent) 
        {
	        return (p.x  < -halfExtent.x ? 0x01 : 0x0) |    
		           (p.x >  halfExtent.x ? 0x08 : 0x0) |
		           (p.y < -halfExtent.y ? 0x02 : 0x0) |    
		           (p.y >  halfExtent.y ? 0x10 : 0x0) |
		           (p.z < -halfExtent.z ? 0x4 : 0x0) |    
		           (p.z >  halfExtent.z ? 0x20 : 0x0);
        }

        public static bool RayAabb(VInt3 rayFrom,
                                 VInt3 rayTo,
                                 VInt3 aabbMin,
                                 VInt3 aabbMax,
					             ref VFixedPoint param, ref VInt3 normal) 
        {
	        VInt3 aabbHalfExtent = (aabbMax - aabbMin) / VFixedPoint.Two;
            VInt3 aabbCenter = (aabbMax + aabbMin) / VFixedPoint.Two;
            VInt3 source = rayFrom - aabbCenter;
            VInt3 target = rayTo - aabbCenter;
            int sourceOutcode = btOutcode(source, aabbHalfExtent);
            int targetOutcode = btOutcode(target, aabbHalfExtent);
	        if ((sourceOutcode & targetOutcode) == 0x0)
	        {
		        VFixedPoint lambda_enter = VFixedPoint.Zero;
                VFixedPoint lambda_exit = param;
                VInt3 r = target - source;
                int i;
                VFixedPoint normSign = VFixedPoint.One;
                VInt3 hitNormal = VInt3.zero;
                int bit = 1;

		        for (int j = 0; j<2;j++)
		        {
			        for (i = 0; i < 3; ++i)
			        {
				        if ((sourceOutcode & bit) != 0)
				        {
					        VFixedPoint lambda = (-source[i] - aabbHalfExtent[i] * normSign) / r[i];
					        if (lambda_enter <= lambda)
					        {
						        lambda_enter = lambda;
						        hitNormal = VInt3.zero;
						        hitNormal[i] = normSign;
					        }
        }
				        else if ((targetOutcode & bit) != 0) 
				        {
					        VFixedPoint lambda = (-source[i] - aabbHalfExtent[i] * normSign) / r[i];

                            lambda_exit = lambda_exit < lambda ? lambda_exit : lambda_exit;
				        }
				        bit<<=1;
			        }
			        normSign = -VFixedPoint.One;
		        }
		        if (lambda_enter <= lambda_exit)
		        {
			        param = lambda_enter;
			        normal = hitNormal;
			        return true;
		        }
	        }
	        return false;
        }
    }
}
