using System;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public interface ISensePropertiesSystem<TGameContext, TSense>: IReadOnlyDynamicDataView3D<SensoryResistance<TSense>>
    {
        event EventHandler<PositionDirtyEventArgs> SenseResistancePositionDirty;
        void OnPositionDirty(object source, PositionDirtyEventArgs args);
        void AddSenseLayerFactory(ISenseLayerFactory<TGameContext, TSense> layerHandler);
        
        bool TryGetSenseLayer(int z, out ISensePropertiesLayer<TGameContext, TSense> data);
        ISensePropertiesLayer<TGameContext, TSense> GetOrCreate(int z);
        void Remove(int z);
    }
}