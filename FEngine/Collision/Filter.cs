using MobaGame.FixedMath;
using System.Collections.Generic;
using System.Text;

namespace MobaGame.Collision
{
    public interface Filter
    {
        bool isAllowed(Filter paramFilter);
    }

    public class DEFAULT_FILTER: Filter
    {
        public bool isAllowed(Filter filter)
        {
            return true;
        }

        public override string ToString()
        {
            return "DefaultFilter[]";
        }
    }
}
