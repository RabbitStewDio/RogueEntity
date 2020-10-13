using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.MapChunks;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public class SensePropertiesDataProcessor<TGameContext, TItemId> : CachableChunkProcessor<TGameContext>,
                                                                       ISensePropertiesDataProcessor<TGameContext>
        where TGameContext : IItemContext<TGameContext, TItemId>, IGridMapContext<TGameContext, TItemId>
        where TItemId : IEntityKey
    {
        readonly int zPosition;

        public SensePropertiesDataProcessor(int width,
                                            int height,
                                            MapLayer layer,
                                            int zPosition,
                                            int spanSizeX,
                                            int spanSizeY) : base(width, height, spanSizeX, spanSizeY)
        {
            this.Layer = layer;
            this.zPosition = zPosition;
            WordSize = 4;
            Data = new byte[width * height * WordSize];
        }

        public byte[] Data { get; }

        public int WordSize { get; }

        public int ZPosition => zPosition;
        public MapLayer Layer { get; }
        
        protected override void Process(TGameContext context, int yStart, int yEnd, int xStart, int xEnd)
        {
            if (!context.TryGetGridDataFor(Layer, out var mapData))
            {
                return;
            }

            if (!mapData.TryGetMap(zPosition, out var groundData, MapAccess.ReadOnly))
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
                        var blockSmell = groundItem.BlocksSmell;
                        Data[offset] = blockLight.ToRawData();
                        Data[offset + 1] = blockSound.ToRawData();
                        Data[offset + 2] = blockHeat.ToRawData();
                        Data[offset + 3] = blockSmell.ToRawData();
                    }
                    else
                    {
                        Data[offset] = 0;
                        Data[offset + 1] = 0;
                        Data[offset + 2] = 0;
                        Data[offset + 3] = 0;
                    }
                }
            }
        }
    }
}