using MobaGame.FixedMath;
using System.Collections.Generic;
using System.Text;

namespace MobaGame.Collision
{
    class DefaultBroadphaseFilter<E, T>: BroadphaseFilterAdapter<E, T>, BroadphaseFilter<E, T> where E :Collidable<T> where T: Fixture
    {
        public override bool isAllowed(E collidable1, T fixture1, E collidable2, T fixture2)
        {
            Filter filter1 = fixture1.getFilter();
            Filter filter2 = fixture2.getFilter();
            return filter1.isAllowed(filter2);
        }
    }
}
