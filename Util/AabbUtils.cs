using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{ 
    public static class AabbUtils
    {
        public static bool RayAabb2(VInt3 rayFrom,
                                  VInt3 rayTo,
                                  VInt3 aabbMin, VInt3 aabbMax,
                                  ref VFixedPoint tmin,
                                  ref VFixedPoint tmax
								  )
        {
            VInt3 d = rayTo - rayFrom;
            VFixedPoint tminf = VFixedPoint.Zero;
            VFixedPoint tmaxf = VFixedPoint.One;
            for(int i = 0; i < 3; i++)
            {
                bool isParallel = d[i].Abs() < Globals.EPS;
                if(isParallel)
                {
                    if ((rayFrom[i] < aabbMin[i] || rayFrom[i] > aabbMax[i]))
                        return false;
                    else
                        continue;
                }
                else
                {
                    VFixedPoint odd = VFixedPoint.One / d[i];
                    VFixedPoint t1 = (aabbMin[i] - rayFrom[i]) * odd;
                    VFixedPoint t2 = (aabbMax[i] - rayFrom[i]) * odd;
                    if(t1 > t2)
                    {
                        VFixedPoint tmp = t1;
                        t1 = t2;
                        t2 = tmp;
                    }

                    tminf = FMath.Max(tminf, t1);
                    tmaxf = FMath.Min(tmaxf, t2);
                }
            }

            tmin = tminf; tmax = tmaxf;
            return !(tmin > tmax || tmin > VFixedPoint.One);
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
	        VInt3 aabbHalfExtent = (aabbMax - aabbMin) * VFixedPoint.Half;
            VInt3 aabbCenter = (aabbMax + aabbMin) * VFixedPoint.Half;
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

                            lambda_exit = lambda_exit < lambda ? lambda_exit : lambda;
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

        public static void transformAabb(VInt3 halfExtents, VFixedPoint margin, VIntTransform trans, out VInt3 aabbMinOut, out VInt3 aabbMaxOut)
        {

            VInt3 halfExtentsWithMargin = halfExtents;
		    halfExtentsWithMargin.x += margin;
		    halfExtentsWithMargin.y += margin;
		    halfExtentsWithMargin.z += margin;

            VInt3 center = trans.position;
            VInt3[] basis = trans.getTransposeBasis();
            VInt3 extent = new VInt3(VInt3.Dot(basis[0].Abs(), halfExtents), VInt3.Dot(basis[1].Abs(), halfExtents), VInt3.Dot(basis[2].Abs(), halfExtents));
            aabbMinOut = center - extent;
            aabbMaxOut = center + extent;
        }
    }
}
