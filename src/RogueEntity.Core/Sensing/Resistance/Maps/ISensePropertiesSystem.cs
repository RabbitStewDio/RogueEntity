using System;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public interface ISensePropertiesSystem<TGameContext, TSense>
    {
        event EventHandler<PositionDirtyEventArgs> SenseResistancePositionDirty;
        void OnPositionDirty(object source, PositionDirtyEventArgs args);
        void AddSenseLayerFactory(ISenseLayerFactory<TGameContext, TSense> layerHandler);
        
        bool TryGetData(int z, out ISensePropertiesLayer<TGameContext, TSense> data);
        ISensePropertiesLayer<TGameContext, TSense> GetOrCreate(int z);
        void Remove(int z);
        
        ReadOnlyListWrapper<int> DefinedZLayers { get; }
        
        int OffsetX { get; }
        int OffsetY { get; }
        int TileSizeX { get; }
        int TileSizeY { get; }
        
    }
}