﻿namespace RogueEntity.Core.Meta.Items
{
    /// <summary>
    ///   A tagging interface telling the system that the trait modifies the
    ///   bulk-item data stored in the ItemReference. Only one such trait can
    ///   exist on a single bulk item or they would overwrite each other's data.
    /// </summary>
    /// <typeparam name="TGameContext"></typeparam>
    /// <typeparam name="TItemId"></typeparam>
    public interface IBulkDataTrait<TGameContext, TItemId> : IBulkItemTrait<TGameContext, TItemId>
    {

    }
}