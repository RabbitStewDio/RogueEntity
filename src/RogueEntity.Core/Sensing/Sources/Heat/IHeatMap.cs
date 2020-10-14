using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing.Sources.Heat
{
    public interface IHeatMap
    {
        bool TryGetHeatData(int z, out ISenseDataView heatMap);
    }
}