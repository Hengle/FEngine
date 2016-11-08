using System;

namespace MobaGame.Collision
{
    class CollisionFilterGroups
    {
        public static readonly short DEFAULT_FILTER = 1;
        public static readonly short STATIC_FILTER = 2;
        public static readonly short KINEMATIC_FILTER = 4;
        public static readonly short DEBRIS_FILTER = 8;
        public static readonly short SENSOR_TRIGGER = 16;
        public static readonly short CHARACTER_FILTER = 32;
        public static readonly short ALL_FILTER = -1; // all bits sets: DefaultFilter | StaticFilter | KinematicFilter | DebrisFilter | SensorTrigger
    }
}
