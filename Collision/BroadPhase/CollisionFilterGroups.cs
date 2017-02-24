using System;

namespace MobaGame.Collision
{
    class CollisionFilterGroups
    {
        public const short DEFAULT_FILTER = 1;
        public const short STATIC_FILTER = 2;
        public const short KINEMATIC_FILTER = 4;
        public const short DEBRIS_FILTER = 8;
        public const short SENSOR_TRIGGER = 16;
        public const short CHARACTER_FILTER = 32;
        public const short ALL_FILTER = -1; // all bits sets: DefaultFilter | StaticFilter | KinematicFilter | DebrisFilter | SensorTrigger
    }
}
