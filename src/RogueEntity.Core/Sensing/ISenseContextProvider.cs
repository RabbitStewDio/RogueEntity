using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Maps;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing
{
    public interface ISenseContextProvider
    {
        public ISenseContext SenseContext { get; }
    }

    public interface ISenseContext
    {
        public IReadOnlyMapData3D<Percentage> BrightnessMap { get; }
        public IReadOnlyMapData3D<SenseProperties> SensePropertyMap { get; }
        bool IsDirty(in EntityGridPosition pos, float radius);
    }
}

