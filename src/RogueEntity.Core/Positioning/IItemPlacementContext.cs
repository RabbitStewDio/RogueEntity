using EnTTSharp.Entities;

namespace RogueEntity.Core.Positioning
{
    public interface IItemPlacementContext<TGameContext, TItemId> 
        where TItemId: IEntityKey
    {
        /// <summary>
        ///   Queries the actor's map layer preference to return a view over all positions/layers
        ///   an actor can be placed in.
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        bool TryGetPositionActionContext(TItemId actor, 
                                         out IItemPlacementDataContext<TGameContext, TItemId> data);
    }

    public interface IItemPlacementDataContext<TGameContext, TItemId>
        where TItemId : IEntityKey
    {
        /// <summary>
        ///   Attemp to remove the item from the map. Returns true if the item was placed on
        ///   the map and returns the old position of the item. Returns false if the item is
        ///   not currently on the map (ie it is contained in a container, inventory etc). 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        bool TryRemoveItem(TItemId item, out Position position);
        bool TryPlaceItem(TItemId item, Position position);
    }
}