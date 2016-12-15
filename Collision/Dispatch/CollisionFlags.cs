namespace MobaGame.Collision
{
    public class CollisionFlags
    {
        public static readonly int NORMAL_OBJECT = 0;

        public static readonly int STATIC_OBJECT = 1;

        /** Sets this collision object as kinematic. */
        public static readonly int KINEMATIC_OBJECT = 2;

        /** Disables contact response. */
        public static readonly int NO_CONTACT_RESPONSE = 4;
    }
}
