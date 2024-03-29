﻿using EnTTSharp.Entities;

namespace RogueEntity.Api.ItemTraits
{
    public interface IReferenceItemTrait<TItemId> : IItemTrait where TItemId : struct, IEntityKey
    {
        /// <summary>
        ///   This method is called right after an character has been spawned. Use this for
        ///   your first time set up.
        /// </summary>
        void Initialize(IEntityViewControl<TItemId> v, TItemId k, IItemDeclaration item);

        /// <summary>
        ///   This method is called after an character's composition has changed. This is called after an
        ///   item has been added or removed from the character and is used to recompute the current
        ///   character stats.
        /// </summary>
        void Apply(IEntityViewControl<TItemId> v, TItemId k, IItemDeclaration item);

        IReferenceItemTrait<TItemId> CreateInstance();
    }
}