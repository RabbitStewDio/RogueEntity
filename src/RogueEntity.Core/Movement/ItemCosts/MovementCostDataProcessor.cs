using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.MapChunks;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Movement.ItemCosts
{
    public static class MovementCostDataProcessor
    {
        public static readonly MovementCostProperties GroundLayerUndefined =
            new MovementCostProperties(MovementCost.Blocked, MovementCost.Normal, MovementCost.Normal, MovementCost.Normal);
        public static readonly MovementCostProperties ItemLayerUndefined =
            new MovementCostProperties(MovementCost.Free, MovementCost.Free, MovementCost.Free, MovementCost.Free);

    }

    public class MovementCostDataProcessor<TGameContext, TItemId> : CachableChunkProcessor<TGameContext>, IMovementCostDataProcessor<TGameContext>
        where TGameContext : IItemContext<TGameContext, TItemId>, IGridMapContext<TGameContext, TItemId>
        where TItemId : IEntityKey
    {
        readonly int depth;
        readonly MovementCostProperties defaultValue;

        public MovementCostDataProcessor(int width, int height,
                                         MapLayer layer, int depth,
                                         int spanSizeX, int spanSizeY, MovementCostProperties defaultValue) : base(width, height, spanSizeX, spanSizeY)
        {
            this.Layer = layer;
            this.depth = depth;
            this.defaultValue = defaultValue;
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

            for (var y = yStart; y < yEnd; y += 1)
            {
                for (var x = xStart; x < xEnd; x += 1)
                {
                    var groundItemRef = groundData[x, y];
                    var offset = (x + y * Width) * 4;
                    var m = ComputeMovement(context, groundItemRef, defaultValue);
                    Data[offset] = m.Walking.RawCost;
                    Data[offset + 1] = m.Flying.RawCost;
                    Data[offset + 2] = m.Ethereal.RawCost;
                    Data[offset + 3] = m.Swimming.RawCost;
                }
            }
        }

        static MovementCostProperties ComputeMovement(TGameContext context,
                                                      TItemId gd,
                                                      MovementCostProperties whenEmpty)
        {
            if (context.ItemResolver.TryQueryData(gd, context, out MovementCostProperties groundProps))
            {
                return groundProps;
            }

            return whenEmpty;
        }
    }

}