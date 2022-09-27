using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using Serilog;

namespace RogueEntity.Core.Positioning.Grid
{
    /// <summary>
    ///    A bruteforce search based location service.
    /// </summary>
    /// <typeparam name="TItemId"></typeparam>
    public class GridItemPlacementLocationService<TItemId> : IItemPlacementLocationService<TItemId>
        where TItemId : struct, IEntityKey
    {
        static readonly ILogger logger = SLog.ForContext<GridItemPlacementService<TItemId>>();
        readonly IItemResolver<TItemId> itemResolver;
        readonly IGridMapContext<TItemId> mapContext;
        readonly IBulkDataStorageMetaData<TItemId> itemIdMetaData;

        public GridItemPlacementLocationService(IItemResolver<TItemId> itemResolver, IGridMapContext<TItemId> mapContext)
        {
            this.itemResolver = itemResolver;
            this.mapContext = mapContext;
            this.itemIdMetaData = itemResolver.EntityMetaData;
        }

        public bool TryFindAvailableSpace(in TItemId itemToBePlaced, in Position origin, out Position placementPos, int searchRadius = 10)
        {
            if (itemIdMetaData.IsReferenceEntity(itemToBePlaced))
            {
                return TryFindEmptySpace(origin, out placementPos, searchRadius);
            }

            if (origin.LayerId == MapLayer.Indeterminate.LayerId)
            {
                logger.Verbose("Given position {Position} has an indeterminate map layer", origin);
                placementPos = default;
                return false;
            }
            
            if (origin.IsInvalid)
            {
                logger.Verbose("Given position {Position} is invalid", origin);
                placementPos = default;
                return false;
            }

            if (!mapContext.TryGetGridDataFor(origin.LayerId, out var mapData))
            {
                logger.Verbose("Unable to resolve grid data for map layer {LayerId} of position {Position}", origin.LayerId, origin);
                placementPos = default;
                return false;
            }

            if (!mapData.TryGetWritableView(origin.GridZ, out var map, DataViewCreateMode.CreateMissing))
            {
                logger.Verbose("Requested grid position for map layer {LayerId} is out of range for position {Position}", origin.LayerId, origin);
                placementPos = default;
                return false;
            }

            var itemStack = itemResolver.QueryStackSize(itemToBePlaced);
            var gx = origin.GridX;
            var gy = origin.GridY;
            var defaultItem = default(TItemId);
            IReadOnlyBoundedDataView<TItemId>? tile = null;
            for (int r = 0; r < searchRadius; r += 1)
            {
                foreach (var c in new Rectangle(gx, gy, searchRadius, searchRadius).PerimeterPositions())
                {
                    if (c.X < 0 || c.Y < 0) continue;

                    placementPos = origin.WithPosition(c.X, c.Y);

                    var itemAtPos = map.TryGetMapValue(ref tile, c.X, c.Y, in defaultItem);
                    if (itemAtPos.IsEmpty)
                    {
                        return true;
                    }

                    if (!itemIdMetaData.IsSameBulkType(itemAtPos, itemToBePlaced))
                    {
                        continue;
                    }

                    var stackSize = itemResolver.QueryStackSize(itemAtPos);
                    if (itemStack.Count + stackSize.Count < stackSize.MaximumStackSize)
                    {
                        return true;
                    }
                }
            }

            placementPos = default;
            return false;
        }

        public bool TryFindEmptySpace(in Position origin, out Position placementPos, int searchRadius = 10)
        {
            if (origin.IsInvalid)
            {
                logger.Verbose("Given position {Position} is invalid", origin);
                placementPos = default;
                return false;
            }

            if (origin.LayerId == MapLayer.Indeterminate.LayerId)
            {
                logger.Verbose("Given position {Position} has an indeterminate map layer", origin);
                placementPos = default;
                return false;
            }


            if (!mapContext.TryGetGridDataFor(origin.LayerId, out var mapData))
            {
                logger.Verbose("Unable to resolve grid data for map layer {LayerId} of position {Position}", origin.LayerId, origin);
                placementPos = default;
                return false;
            }

            if (!mapData.TryGetWritableView(origin.GridZ, out var map, DataViewCreateMode.CreateMissing))
            {
                logger.Verbose("Requested grid position for map layer {LayerId} is out of range for position {Position}", origin.LayerId, origin);
                placementPos = default;
                return false;
            }

            var gx = origin.GridX;
            var gy = origin.GridY;
            var defaultValue = default(TItemId);
            IReadOnlyBoundedDataView<TItemId>? tile = null;

            for (int r = 0; r < searchRadius; r += 1)
            {
                foreach (var c in new Rectangle(gx, gy, searchRadius, searchRadius).PerimeterPositions())
                {
                    if (c.X < 0 || c.Y < 0) continue;

                    placementPos = origin.WithPosition(c.X, c.Y);

                    var data = map.TryGetMapValue(ref tile, c.X, c.Y, in defaultValue);
                    if (data.IsEmpty)
                    {
                        return true;
                    }
                }
            }

            placementPos = default;
            return false;
        }

    }
}
