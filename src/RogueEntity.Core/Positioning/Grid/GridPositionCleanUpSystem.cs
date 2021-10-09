using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Base;
using System.Collections.Generic;

namespace RogueEntity.Core.Positioning.Grid
{
    public class GridPositionCleanUpSystem<TEntityId, TPosition>
        where TEntityId : IEntityKey
        where TPosition: IPosition<TPosition>
    {
        readonly List<(TEntityId k, TPosition pos)> removedEntities;
        readonly IItemPlacementServiceContext<TEntityId> placementService;

        public GridPositionCleanUpSystem(IItemPlacementServiceContext<TEntityId> placementService)
        {
            this.removedEntities = new List<(TEntityId, TPosition)>();
            this.placementService = placementService;
        }

        public void StartCollection()
        {
            removedEntities.Clear();
        }

        public void CollectDestroyedEntities(IEntityViewControl<TEntityId> v, TEntityId k, in DestroyedMarker m, in TPosition pos)
        {
            if (pos.IsInvalid)
            {
                return;
            }

            removedEntities.Add((k, pos));
        }

        public void RemoveCollectedEntitiesFromMap()
        {
            foreach (var (k, pos) in removedEntities)
            {
                if (placementService.TryGetItemPlacementService(pos.LayerId, out var service))
                {
                    service.TryRemoveItem(k, pos);
                }
            }
        }
    }
}
