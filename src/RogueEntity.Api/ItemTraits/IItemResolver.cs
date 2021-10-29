using EnTTSharp.Entities;

namespace RogueEntity.Api.ItemTraits
{
    public interface IItemResolver<TItemId>
    {
        IBulkDataStorageMetaData<TItemId> EntityMetaData { get; }
        IItemRegistry ItemRegistry { get; }

        bool TryResolve(in TItemId itemRef, out IItemDeclaration item);

        TItemId Instantiate(IItemDeclaration item);

        bool TryQueryTrait<TItemData>(TItemId itemRef, out TItemData data) where TItemData: IItemTrait;

        bool TryQueryData<TData>(TItemId itemRef, out TData data);

        bool TryUpdateData<TData>(TItemId itemRef,
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
        /// <param name="changedItem"></param>
        /// <typeparam name="TData"></typeparam>
        /// <returns></returns>
        bool TryRemoveData<TData>(TItemId itemRef, out TItemId changedItem);

        void DiscardUnusedItem(in TItemId item);
        void Apply(TItemId reference);
        
        /// <summary>
        ///   Destroys the given entity at the end of the current turn.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        TItemId Destroy(in TItemId item);
        
        /// <summary>
        ///   Destroys the given entity at the end of the next turn.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        TItemId DestroyNext(in TItemId item);

        /// <summary>
        ///   Returns true if this item is a reference item that has been marked as destroyed
        ///   or that has been destroyed already.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool IsDestroyed(in TItemId item);
        
        IReferenceEntityQueryProvider<TItemId> QueryProvider { get; }
    }
}