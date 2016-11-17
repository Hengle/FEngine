using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class DbvtBroadphase: BroadphaseInterface
    {
        public static readonly float DBVT_BP_MARGIN = 0.05f;

        public static readonly int DYNAMIC_SET = 0; // Dynamic set index
        public static readonly int FIXED_SET   = 1; // Fixed set index
        public static readonly int STAGECOUNT  = 2; // Number of stages

        public readonly Dbvt[] sets = new Dbvt[2];                        // Dbvt sets
        public DbvtProxy[] stageRoots = new DbvtProxy[STAGECOUNT + 1]; // Stages list
        public OverlappingPairCache paircache;                         // Pair cache
        public float predictedframes;                                  // Frames predicted
        public int stageCurrent;                                       // Current stage
        public int fupdates;                                           // % of fixed updates per frame
        public int dupdates;                                           // % of dynamic updates per frame
        public int pid;                                                // Parse id
        public int gid;                                                // Gen id
        public bool releasepaircache;                               // Release pair cache on delete

    }
}