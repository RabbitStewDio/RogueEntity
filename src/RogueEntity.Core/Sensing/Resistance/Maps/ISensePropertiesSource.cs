using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public interface ISensePropertiesSource
    {
        bool TryGet(int z, out IReadOnlyMapData<SenseProperties> data);
    }
}