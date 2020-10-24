using GoRogue;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;
using Rectangle = RogueEntity.Core.Utils.Rectangle;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    /// <summary>
    ///   A Tagging interface to prevent me from doing silly things when combining data processors
    ///   of unrelated kinds.
    /// </summary>
    /// <typeparam name="TGameContext"></typeparam>
    public interface ISensePropertiesDataProcessor<TGameContext>
    {
        MapLayer Layer { get; }
        int ZPosition { get; }
        DynamicDataView<SensoryResistance> Data { get; }
        ReadOnlyListWrapper<Rectangle> ProcessedTiles { get; }
        
        void MarkDirty(int posGridX, int posGridY);
        void ResetDirtyFlags();
        bool Process(TGameContext context);
    }
}