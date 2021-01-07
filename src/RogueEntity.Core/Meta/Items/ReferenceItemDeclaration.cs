using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;

namespace RogueEntity.Core.Meta.Items
{
    public class ReferenceItemDeclaration<TContext, TItemId> : ItemDeclaration<TContext>, 
                                                               IReferenceItemDeclaration<TContext, TItemId> 
        where TItemId : IEntityKey
    {
        readonly TraitRegistration<IReferenceItemTrait<TContext, TItemId>> traits;

        public ReferenceItemDeclaration(ItemDeclarationId id): this(id, id.Id) 
        {
        }

        public ReferenceItemDeclaration(ItemDeclarationId id, string tag): base(id, tag) 
        {
            traits = new TraitRegistration<IReferenceItemTrait<TContext, TItemId>>(TraitComparer.Default);
        }

        public ReferenceItemDeclaration<TContext, TItemId> WithTrait(IReferenceItemTrait<TContext, TItemId> trait)
        {
            traits.Add(trait);
            return this;
        }

        public override bool TryQuery<TTrait>(out TTrait t)
        {
            return traits.TryQuery(out t);
        }

        public override BufferList<TTrait> QueryAll<TTrait>(BufferList<TTrait> cache = null)
        {
            return traits.QueryAll(cache);
        }

        public virtual void Initialize(IEntityViewControl<TItemId> v, TContext context, TItemId k)
        {
            foreach (var itemTrait in traits)
            {
                itemTrait.Initialize(v, context, k, this);
            }
        }

        public virtual void Apply(IEntityViewControl<TItemId> v, TContext context, TItemId k)
        {
            foreach (var itemTrait in traits)
            {
                itemTrait.Apply(v, context, k, this);
            }
        }

    }
}