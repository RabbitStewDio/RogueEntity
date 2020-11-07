using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    /// <summary>
    ///   A Tagging interface to prevent me from doing silly things when combining data processors
    ///   of unrelated kinds.
    /// </summary>
    /// <typeparam name="TGameContext"></typeparam>
    /// <typeparam name="TSense"></typeparam>
    public interface ISensePropertiesDataProcessor<TGameContext, TSense>
    {
        MapLayer Layer { get; }
        int ZPosition { get; }
        IReadOnlyDynamicDataView2D<SensoryResistance<TSense>> Data { get; }
        ReadOnlyListWrapper<Rectangle> ProcessedTiles { get; }
        
        void MarkDirty(int posGridX, int posGridY);
        void ResetDirtyFlags();
        bool Process(TGameContext context);
    }
}