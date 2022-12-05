using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Movement.CostModifier.Map
{
    public class RelativeMovementCostSystem<TMovementType> : LayeredAggregationSystem<float, RelativeMovementCostModifier<TMovementType>>,
                                                             IRelativeMovementCostSystem<TMovementType>
    {
        public RelativeMovementCostSystem(int tileWidth, int tileHeight) : base(RelativeMovementCostSystem.ProcessTile, tileWidth, tileHeight)
        { }

        public RelativeMovementCostSystem(int offsetX, int offsetY, int tileSizeX, int tileSizeY) : base(RelativeMovementCostSystem.ProcessTile, offsetX, offsetY, tileSizeX, tileSizeY)
        { }
    }

    public static class RelativeMovementCostSystem
    {
        public static void ProcessTile<TMovementType>(AggregationProcessingParameter<float, RelativeMovementCostModifier<TMovementType>> p)
        {
            var bounds = p.Bounds;
            var resistanceData = p.WritableTile;
            resistanceData.Fill(RelativeMovementCostModifier<TMovementType>.Unchanged);

            foreach (var view in p.DataViews)
            {
                if (!view.TryGetData(bounds.X, bounds.Y, out var dv))
                {
                    continue;
                }

                foreach (var (x, y) in bounds.Contents)
                {
                    if (!dv.TryGet(x, y, out var costModifier) ||
                        !resistanceData.TryGet(x, y, out var resistance))
                    {
                        continue;
                    }

                    var sp = resistance + costModifier;
                    resistanceData.TrySet(x, y, sp);
                }
            }
        }

        public static void AddLayer<TItemId, TMovementMode>(this IAggregationLayerSystemBackend<RelativeMovementCostModifier<TMovementMode>> system,
                                                            IMapContext<TItemId> mapContext,
                                                            IItemResolver<TItemId> itemContext,
                                                            MapLayer mapLayer)
            where TItemId : struct, IEntityKey
        {
            system.AddSenseLayerFactory(new RelativeMovementCostLayerFactory<TItemId, TMovementMode>(mapLayer, mapContext, itemContext));
        }
    }
}
