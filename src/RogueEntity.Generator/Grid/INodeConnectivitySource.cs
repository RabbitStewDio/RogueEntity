using RogueEntity.Generator.MapFragments;
using ValionRL.Core.MapFragments;

namespace ValionRL.Core.Generator
{
    public interface INodeConnectivitySource
    {
        bool CanConnectTo(int x, int y, MapFragmentConnectivity edge, bool whenNoNode = true);
    }
}