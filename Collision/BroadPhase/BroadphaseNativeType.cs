﻿using MobaGame.FixedMath;
using System.Text;

namespace MobaGame.Collision
{
    public enum BroadphaseNativeType
    { // polyhedral convex shapes:
        BOX_SHAPE_PROXYTYPE,
        TRIANGLE_SHAPE_PROXYTYPE,
        TETRAHEDRAL_SHAPE_PROXYTYPE,
        CONVEX_TRIANGLEMESH_SHAPE_PROXYTYPE,
        CONVEX_HULL_SHAPE_PROXYTYPE,

        // implicit convex shapes:
        IMPLICIT_CONVEX_SHAPES_START_HERE,
        SPHERE_SHAPE_PROXYTYPE,
        MULTI_SPHERE_SHAPE_PROXYTYPE,
        CAPSULE_SHAPE_PROXYTYPE,
        CONE_SHAPE_PROXYTYPE,
        CONVEX_SHAPE_PROXYTYPE,
        CYLINDER_SHAPE_PROXYTYPE,
        UNIFORM_SCALING_SHAPE_PROXYTYPE,
        MINKOWSKI_SUM_SHAPE_PROXYTYPE,
        MINKOWSKI_DIFFERENCE_SHAPE_PROXYTYPE,

        // concave shapes:
        CONCAVE_SHAPES_START_HERE,

        // keep all the convex shapetype below here, for the check IsConvexShape in broadphase proxy!
        TRIANGLE_MESH_SHAPE_PROXYTYPE,
        SCALED_TRIANGLE_MESH_SHAPE_PROXYTYPE,

        // used for demo integration FAST/Swift collision library and Bullet:
        FAST_CONCAVE_MESH_PROXYTYPE,

        // terrain:
        TERRAIN_SHAPE_PROXYTYPE,

        // used for GIMPACT Trimesh integration:
        GIMPACT_SHAPE_PROXYTYPE,

        // multimaterial mesh:
        MULTIMATERIAL_TRIANGLE_MESH_PROXYTYPE,

        EMPTY_SHAPE_PROXYTYPE,
        STATIC_PLANE_PROXYTYPE,
        CONCAVE_SHAPES_END_HERE,
        COMPOUND_SHAPE_PROXYTYPE,

        SOFTBODY_SHAPE_PROXYTYPE,

        INVALID_SHAPE_PROXYTYPE,

        MAX_BROADPHASE_COLLISION_TYPES
    }
}

