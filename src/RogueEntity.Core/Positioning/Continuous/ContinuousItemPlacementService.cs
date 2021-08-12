using System;

namespace RogueEntity.Core.Positioning.Continuous
{
    public class ContinuousItemPlacementService<TItemId>: IItemPlacementService<TItemId>
    {
        public event EventHandler<ItemPositionChangedEvent<TItemId>> ItemPositionChanged;

        public bool TryQueryItem<TPosition>(in TPosition pos, out TItemId item)
            where TPosition : IPosition<TPosition>
        {
            throw new System.NotImplementedException();
        }

        public bool TryRemoveItem<TPosition>(in TItemId targetItem, in TPosition placementPos)
            where TPosition : IPosition<TPosition>
        {
            throw new System.NotImplementedException();
        }

        public bool TryPlaceItem<TPosition>(in TItemId targetItem, in TPosition placementPos)
            where TPosition : IPosition<TPosition>
        {
            throw new System.NotImplementedException();
        }

        public bool TryMoveItem<TPosition>(in TItemId item, in TPosition currentPos, in TPosition placementPos)
            where TPosition : IPosition<TPosition>
        {
            throw new System.NotImplementedException();
        }

        public bool TrySwapItem<TPosition>(in TItemId sourceItem, in TPosition sourcePosition, in TItemId targetItem, in TPosition targetPosition)
            where TPosition : IPosition<TPosition>
        {
            throw new System.NotImplementedException();
        }
    }

    public class ContinuousItemPlacementLocationService<TItemId>: IItemPlacementLocationService<TItemId>
    {
        public bool TryFindAvailableSpace(in TItemId itemToBePlaced, in Position origin, out Position placementPos, int searchRadius = 10)
        {
            throw new System.NotImplementedException();
        }

        public bool TryFindEmptySpace(in Position origin, out Position placementPos, int searchRadius = 10)
        {
            throw new System.NotImplementedException();
        }
    }
}
