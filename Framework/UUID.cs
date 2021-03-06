﻿namespace MobaGame.Collision
{
    public struct UUID 
    {
        static int NextUUID = 0;

        public int id;

        public static UUID GetNextUUID()
        {
            UUID nextID = new UUID();
            nextID.id = NextUUID++;
            return nextID;
        }

        public override string ToString()
        {
            return id.ToString();
        }

        public override int GetHashCode()
        {
            return id;
        }

        public override bool Equals(object obj)
        {
            if(obj is UUID)
            {
                UUID another = (UUID)obj;
                return another.id == id;
            }
            return false;
        }

        public static bool operator<(UUID lhs, UUID rhs)
        {
            return lhs.id < rhs.id;
        }

        public static bool operator>(UUID lhs, UUID rhs)
        {
            return lhs.id > rhs.id;
        }

        public static bool operator==(UUID lhs, UUID rhs)
        {
            return lhs.id == rhs.id;
        }

        public static bool operator!=(UUID lhs, UUID rhs)
        {
            return lhs.id != rhs.id;
        }

        public static bool operator <=(UUID lhs, UUID rhs)
        {
            return lhs.id <= rhs.id;
        }

        public static bool operator >=(UUID lhs, UUID rhs)
        {
            return lhs.id >= rhs.id;
        }
    }
}