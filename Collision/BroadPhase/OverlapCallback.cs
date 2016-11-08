namespace MobaGame.Collision
{
    public abstract class OverlapCallback
    {
        public abstract bool processOverlap(BroadphasePair pair);
    }
}
