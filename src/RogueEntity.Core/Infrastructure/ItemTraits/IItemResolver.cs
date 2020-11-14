using EnTTSharp.Entities;

namespace RogueEntity.Core.Infrastructure.ItemTraits
{
    public interface IItemResolver<TGameContext, TItemId> where TItemId: IEntityKey
    {
        IItemRegistry ItemRegistry { get; }

        bool TryResolve(in TItemId itemRef, out IItemDeclaration item);

        TItemId Instantiate(TGameContext context, IItemDeclaration item);

        bool TryQueryTrait<TItemData>(TItemId itemRef, out TItemData data) where TItemData: IItemTrait;

        bool TryQueryData<TData>(TItemId itemRef, TGameContext context, out TData data);

        bool TryUpdateData<TData>(TItemId itemRef,
                                  TGameContext context,
                                  in TData data,
                                  out TItemId changedItem);

        /// <summary>
        ///   Attempts to remove any associated data from the given entity. This only works
        ///   well on reference items, as bulk items are expected to encode a fixed pattern
        ///   of data in the key itself. 
        ///
        ///   This will return true if the entity no longer stores data of the given type,
        ///   regardless of whether the data itself has been removed previously.
        /// </summary>
        /// <param name="itemRef"></param>
        /// <param name="context"></param>
        /// <param name="changedItem"></param>
        /// <typeparam name="TData"></typeparam>
        /// <returns></returns>
        bool TryRemoveData<TData>(TItemId itemRef, TGameContext context, out TItemId changedItem);

        void DiscardUnusedItem(in TItemId item);
        void Apply(TItemId reference, TGameContext context);
        TItemId Destroy(in TItemId item);
        TItemId DestroyNext(in TItemId item);
    }
}