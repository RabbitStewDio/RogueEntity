using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.MapChunks;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    /// <summary>
    ///   A Tagging interface to prevent me from doing silly things when combining data processors
    ///   of unrelated kinds.
    /// </summary>
    /// <typeparam name="TGameContext"></typeparam>
    public interface ISensePropertiesDataProcessor<TGameContext> : ICachableChunkProcessor<TGameContext>, IByteBlitterDataSource
    {
        int Width { get; }
        int Height { get; }
        
        MapLayer Layer { get; }
        int ZPosition { get; }
    }
}