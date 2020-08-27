using EnTTSharp.Entities;

namespace RogueEntity.Core.Meta.UseEffects
{
    /// <summary>
    ///    Represents a generic action that happens when an item is used.
    ///    Item effects are declared on the item to be used.
    ///
    ///    This is a single object system - "use [door]". Unlike a two
    ///    operand systems ("use [key] on [door]"), this system assumes we don't
    ///    want to let users waste time on trying every item combination.
    ///
    ///    Use the GameContext to query your surroundings to establish a sensible
    ///    context where needed. ("Use [door]" if locked may simply search the
    ///    actor's inventory for the right key etc.)
    /// </summary>
    /// <typeparam name="TGameContext"></typeparam>
    /// <typeparam name="TActorId"></typeparam>
    /// <typeparam name="TItemId"></typeparam>
    public interface IUsableItemEffect<TGameContext, TActorId, TItemId>
        where TItemId : IEntityKey
        where TActorId : IEntityKey
    {
        bool TryActivate(TActorId user, TGameContext context, TItemId itemToBeUsed, out TItemId usedItem);
    }
}