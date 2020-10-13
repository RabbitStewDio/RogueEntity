using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public interface ISensePropertiesSystem<TGameContext>
    {
        void OnPositionDirty(object source, PositionDirtyEventArgs args);
        void AddSenseLayerFactory(ISenseLayerFactory<TGameContext> layerHandler);
        
        bool TryGet(int z, out SensePropertiesMap<TGameContext> data);
        bool TryGetOrCreate(int z, int width, int height, out SensePropertiesMap<TGameContext> data);
        void Remove(int z);
        
        ReadOnlyListWrapper<int> DefinedLayers { get; }
    }
}