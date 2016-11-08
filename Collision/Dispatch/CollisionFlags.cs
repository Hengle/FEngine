namespace MobaGame.Collision
{
    public class CollisionFlags
    {
        public static readonly int STATIC_OBJECT = 1;

        /** Sets this collision object as kinematic. */
        public static readonly int KINEMATIC_OBJECT = 2;

        /** Disables contact response. */
        public static readonly int NO_CONTACT_RESPONSE = 4;

        /**
         * Enables calling {@link ContactAddedCallback} for collision objects. This
         * allows per-triangle material (friction/restitution).
         */
        public static readonly int CUSTOM_MATERIAL_CALLBACK = 8;

        public static readonly int CHARACTER_OBJECT = 16;
    }
}
