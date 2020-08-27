using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.MapChunks;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Maps
{
    /// <summary>
    ///   A Tagging interface to prevent me from doing silly things when combining data processors
    ///   of unrelated kinds.
    /// </summary>
    /// <typeparam name="TGameContext"></typeparam>
    public interface ISenseMapDataProcessor<TGameContext> : ICachableChunkProcessor<TGameContext>
    {

    }

    public class SenseMapDataProcessor<TGameContext, TItemId> : CachableChunkProcessor<TGameContext>, ISenseMapDataProcessor<TGameContext>
        where TGameContext: IItemContext<TGameContext, TItemId>, IGridMapContext<TGameContext, TItemId>
        where TItemId : IEntityKey
    {
        readonly int depth;

        public SenseMapDataProcessor(int width, int height, 
                                     MapLayer layer, int depth,
                                     int spanSizeX, int spanSizeY) : base(width, height, spanSizeX, spanSizeY)
        {
            this.Layer = layer;
            this.depth = depth;
        }

        public MapLayer Layer { get; }

        protected override void Process(TGameContext context, int yStart, int yEnd, int xStart, int xEnd)
        {
            if (!context.TryGetGridDataFor(Layer, out var mapData))
            {
                return;
            }

            if (!mapData.TryGetMap(depth, out var groundData))
            {
                return;
            }

            var itemResolver = context.ItemResolver;
            for (var y = yStart; y < yEnd; y += 1)
            {
                for (var x = xStart; x < xEnd; x += 1)
                {
                    var offset = (x + y * Width) * 4;
                    var groundItemRef = groundData[x, y];
                    if (itemResolver.TryQueryData(groundItemRef, context, out SensoryResistance groundItem))
                    {
                        var blockLight = groundItem.BlocksLight;
                        var blockSound = groundItem.BlocksSound;
                        var blockHeat = groundItem.BlocksHeat;
                        Data[offset] = blockLight.ToRawData();
                        Data[offset + 1] = blockSound.ToRawData();
                        Data[offset + 2] = blockHeat.ToRawData();
                    }
                    else
                    {
                        Data[offset] = 0;
                        Data[offset + 1] = 0;
                        Data[offset + 2] = 0;
                    }
                }
            }
        }

    }
}