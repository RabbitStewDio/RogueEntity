using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;

namespace RogueEntity.Core.Meta.Items
{
    public class BulkItemDeclaration<TContext, TItemId> : ItemDeclaration<TContext>, IBulkItemDeclaration<TContext, TItemId> 
        where TItemId : IEntityKey
    {
        readonly TraitRegistration<IBulkItemTrait<TContext, TItemId>> traitRegistration;

        public BulkItemDeclaration(ItemDeclarationId id) : this(id, id.Id) {}

        public BulkItemDeclaration(ItemDeclarationId id, string tag) :
            base(id, tag)
        {
            traitRegistration = new TraitRegistration<IBulkItemTrait<TContext, TItemId>>(TraitComparer.Default);
        }

        public BulkItemDeclaration<TContext, TItemId> WithTrait(IBulkItemTrait<TContext, TItemId> trait)
        {
            EnsureSingleInstanceOfBulkDataTrait(trait);
            traitRegistration.Add(trait);
            return this;
        }

        public BulkItemDeclaration<TContext, TItemId> WithoutTrait<TTrait>()
        {
            traitRegistration.Remove<TTrait>();
            return this;
        }

        void EnsureSingleInstanceOfBulkDataTrait(IBulkItemTrait<TContext, TItemId> trait)
        {
            if (trait is IBulkDataTrait<TContext, TItemId>)
            {
                if (traitRegistration.TryQuery<IBulkDataTrait<TContext, TItemId>>(out _))
                {
                    throw new ArgumentException("Only one bulk data trait can be added to an item.");
                }
            }
        }

        public override bool TryQuery<TTrait>(out TTrait t)
        {
            return traitRegistration.TryQuery(out t);
        }

        public override BufferList<TTrait> QueryAll<TTrait>(BufferList<TTrait> cache = null)
        {
            return traitRegistration.QueryAll(cache);
        }

        public virtual TItemId Initialize(TContext context, TItemId itemReference)
        {
            foreach (var itemTrait in traitRegistration)
            {
                itemReference = itemTrait.Initialize(context, this, itemReference);
            }

            return itemReference;
        }
    }

}