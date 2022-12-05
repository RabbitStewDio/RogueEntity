using RogueEntity.Api.Utils;
using System;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Positioning
{
    public static class ItemPositionChangedEvent
    {
        public static ItemPositionChangedEvent<TItemId> ForRemove<TItemId, TPosition>(TItemId item, TPosition pos)
            where TPosition : IPosition<TPosition>
        {
            return new ItemPositionChangedEvent<TItemId>(item, Position.From(pos), default);
        }
        
        public static ItemPositionChangedEvent<TItemId> ForMove<TItemId, TPosition>(TItemId item, TPosition posFrom, TPosition posTo)
            where TPosition : IPosition<TPosition>
        {
            return new ItemPositionChangedEvent<TItemId>(item, Position.From(posFrom), Position.From(posTo));
        }
        
        public static ItemPositionChangedEvent<TItemId> ForPlacement<TItemId, TPosition>(TItemId item, TPosition posTo)
            where TPosition : IPosition<TPosition>
        {
            return new ItemPositionChangedEvent<TItemId>(item, default, Position.From(posTo));
        }
    } 
    
    public readonly struct ItemPositionChangedEvent<TItemId>
    {
        public readonly TItemId Item;
        public readonly Position SourcePosition;
        public readonly Position TargetPosition;

        public ItemPositionChangedEvent(TItemId item, Position sourcePosition, Position targetPosition)
        {
            Item = item;
            SourcePosition = sourcePosition;
            TargetPosition = targetPosition;
        }
    }

    /// <summary>
    ///   Encapsulates all primitive operations that place (bulk and reference) entities 
    ///   into a map.
    /// </summary>
    /// <typeparam name="TItemId"></typeparam>
    public interface IItemPlacementService<TItemId>
    {
        event EventHandler<ItemPositionChangedEvent<TItemId>> ItemPositionChanged;

        /// <summary>
        ///   Tries to query the first item at the given position. 
        /// </summary>
        /// <param name="placementPos"></param>
        /// <param name="item"></param>
        /// <typeparam name="TPosition"></typeparam>
        /// <returns></returns>
        bool TryQueryItem<TPosition>(in TPosition placementPos, [MaybeNullWhen(false)] out TItemId item)
            where TPosition : struct, IPosition<TPosition>;

        /// <summary>
        ///   Tries to query all items at the given position. 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="buffer"></param>
        /// <typeparam name="TPosition"></typeparam>
        /// <returns></returns>
        BufferList<TItemId> QueryItems<TPosition>(in TPosition pos, BufferList<TItemId>? buffer = null)
            where TPosition : struct, IPosition<TPosition>;

        /// <summary>
        ///   Tries to remove the target item from the map. The item will not be destroyed
        ///   in the process. Use this to place items into containers or generally leave
        ///   them outside of the physical realm.
        /// </summary>
        /// <param name="targetItem"></param>
        /// <param name="placementPos"></param>
        /// <returns></returns>
        bool TryRemoveItem<TPosition>(in TItemId targetItem,
                                      in TPosition placementPos)
            where TPosition : struct, IPosition<TPosition>;

        /// <summary>
        ///    Places an item at the given placement position. It is assumed and strongly
        ///    recommended that the item has not been placed elsewhere (and is validated for
        ///    reference items).
        /// </summary>
        /// <param name="targetItem"></param>
        /// <param name="placementPos"></param>
        /// <returns></returns>
        bool TryPlaceItem<TPosition>(in TItemId targetItem,
                                     in TPosition placementPos)
            where TPosition : struct, IPosition<TPosition>;

        /// <summary>
        ///    Moves an item at the given current position to the placement position. This
        ///    method will fail if the target position is not empty or stackable and receptive
        ///    of the given item.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="currentPos"></param>
        /// <param name="placementPos"></param>
        /// <returns></returns>
        bool TryMoveItem<TPosition>(in TItemId item,
                                    in TPosition currentPos,
                                    in TPosition placementPos)
            where TPosition : struct, IPosition<TPosition>;

        /// <summary>
        ///    Swaps the source entity with the target entity (which is expected to be located at the given position).
        /// </summary>
        bool TrySwapItem<TPosition>(in TItemId sourceItem,
                                    in TPosition sourcePosition,
                                    in TItemId targetItem,
                                    in TPosition targetPosition)
            where TPosition : struct, IPosition<TPosition>;
    }
}
