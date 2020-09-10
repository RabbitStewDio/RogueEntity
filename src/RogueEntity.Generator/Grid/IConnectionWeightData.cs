using RogueEntity.Generator.MapFragments;
using ValionRL.Core.MapFragments;

namespace ValionRL.Core.Generator
{
    public interface IConnectionWeightData
    {
        void UpdateConnectionWeight(MapFragmentConnectivity c, int weight);
    }
}