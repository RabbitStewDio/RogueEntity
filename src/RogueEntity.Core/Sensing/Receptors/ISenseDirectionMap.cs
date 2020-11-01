using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing.Receptors
{
    public interface ISenseDirectionMap
    {
        bool TryGetSenseData(int z, out IDynamicSenseDataView2D intensities);
    }
}