﻿using EnTTSharp.Entities;

namespace RogueEntity.Core.Meta.Items
{
    public interface IItemResolver<TContext, TItemId> where TItemId: IEntityKey
    {
        IItemRegistry ItemRegistry { get; }

        bool TryResolve(in TItemId itemRef, out IItemDeclaration item);

        TItemId Instantiate(TContext context, IItemDeclaration item);

        bool TryQueryTrait<TItemData>(TItemId itemRef, out TItemData data) where TItemData: IItemTrait;

        bool TryQueryData<TData>(TItemId itemRef, TContext context, out TData data);

        bool TryUpdateData<TData>(TItemId itemRef,
                                  TContext context,
                                  in TData data,
                                  out TItemId changedItem);

        bool TryRemoveData<TData>(TItemId itemRef, TContext context, out TItemId changedItem);

        void DiscardUnusedItem(in TItemId item);
        void Apply(TItemId reference, TContext context);
        TItemId Destroy(in TItemId item);
    }
}