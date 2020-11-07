using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public interface ISensePropertiesLayer<TGameContext, TSense>
    {
        bool IsDefined(MapLayer layer);
        void AddProcess(MapLayer layer, ISensePropertiesDataProcessor<TGameContext, TSense> p);
        void RemoveLayer(MapLayer layer);
    }
}