using System;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Meta.Items
{
    public class BulkItemDeclaration<TItemId> : ItemDeclaration, IBulkItemDeclaration<TItemId> 
        where TItemId : struct, IEntityKey
    {
        readonly TraitRegistration<IBulkItemTrait<TItemId>> traitRegistration;

        public BulkItemDeclaration(ItemDeclarationId id) : this(id, new WorldEntityTag(id.Id)) {}

        public BulkItemDeclaration(ItemDeclarationId id, WorldEntityTag tag) :
            base(id, tag)
        {
            traitRegistration = new TraitRegistration<IBulkItemTrait<TItemId>>(TraitComparer.Default);
            traitRegistration.Add(new DefaultEntityTagTrait<TItemId>(tag));
        }

        public IBulkItemDeclaration<TItemId> WithTrait(IBulkItemTrait<TItemId> trait)
        {
            EnsureSingleInstanceOfBulkDataTrait(trait);
            traitRegistration.Add(trait);
            return this;
        }

        public IBulkItemDeclaration<TItemId> WithoutTrait<TTrait>()
        {
            traitRegistration.Remove<TTrait>();
            return this;
        }

        void EnsureSingleInstanceOfBulkDataTrait(IBulkItemTrait<TItemId> trait)
        {
            if (trait is IBulkDataTrait<TItemId>)
            {
                if (traitRegistration.TryQuery<IBulkDataTrait<TItemId>>(out _))
                {
                    throw new ArgumentException("Only one bulk data trait can be added to an item.");
                }
            }
        }

        public override bool TryQuery<TTrait>([MaybeNullWhen(false)] out TTrait t)
        {
            return traitRegistration.TryQuery(out t);
        }

        public override BufferList<TTrait> QueryAll<TTrait>(BufferList<TTrait>? cache = null)
        {
            return traitRegistration.QueryAll(cache);
        }

        public virtual TItemId Initialize(TItemId itemReference)
        {
            foreach (var itemTrait in traitRegistration)
            {
                itemReference = itemTrait.Initialize(this, itemReference);
            }

            return itemReference;
        }
    }

}