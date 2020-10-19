using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Receptors.InfraVision;

namespace RogueEntity.Core.Sensing.Receptors
{
    public interface ISenseDirectionMap
    {
        bool TryGetSenseData(int z, out ISenseDataView intensities);
    }
}