namespace MobaGame.Collision
{
    public class SolverMode
    {
        public readonly static int SOLVER_RANDMIZE_ORDER = 1;
        public readonly static int SOLVER_FRICTION_SEPARATE = 2;
        public readonly static int SOLVER_USE_WARMSTARTING = 4;
        public readonly static int SOLVER_USE_2_FRICTION_DIRECTIONS = 16;
        public readonly static int SOLVER_ENABLE_FRICTION_DIRECTION_CACHING = 32;
        public readonly static int SOLVER_DISABLE_VELOCITY_DEPENDENT_FRICTION_DIRECTION = 64;
        public readonly static int SOLVER_CACHE_FRIENDLY = 128;
        public readonly static int SOLVER_SIMD = 256;
        public readonly static int SOLVER_INTERLEAVE_CONTACT_AND_FRICTION_CONSTRAINTS = 512;
        public readonly static int SOLVER_ALLOW_ZERO_LENGTH_FRICTION_DIRECTIONS = 1024;
    }
}