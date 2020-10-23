using System;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public interface ISensePropertiesSystem<TGameContext>
    {
        event EventHandler<PositionDirtyEventArgs> SenseResistancePositionDirty;
        void OnPositionDirty(object source, PositionDirtyEventArgs args);
        void AddSenseLayerFactory(ISenseLayerFactory<TGameContext> layerHandler);
        
        bool TryGetData(int z, out ISensePropertiesLayer<TGameContext> data);
        ISensePropertiesLayer<TGameContext> GetOrCreate(int z);
        void Remove(int z);
        
        ReadOnlyListWrapper<int> DefinedZLayers { get; }
        
        int OffsetX { get; }
        int OffsetY { get; }
        int TileWidth { get; }
        int TileHeight { get; }
        
    }
}