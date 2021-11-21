using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;

namespace RogueEntity.Core.Meta.Items
{
    public class ReferenceItemDeclaration<TItemId> : ItemDeclaration,
                                                     IReferenceItemDeclaration<TItemId>
        where TItemId : IEntityKey
    {
        readonly TraitRegistration<IReferenceItemTrait<TItemId>> traits;

        public ReferenceItemDeclaration(ItemDeclarationId id) : this(id, new WorldEntityTag(id.Id))
        { }

        public ReferenceItemDeclaration(ItemDeclarationId id, WorldEntityTag tag) : base(id, tag)
        {
            traits = new TraitRegistration<IReferenceItemTrait<TItemId>>(TraitComparer.Default);
            traits.Add(new DefaultEntityTagTrait<TItemId>(tag));
        }

        public IReferenceItemDeclaration<TItemId> WithTrait(IReferenceItemTrait<TItemId> trait)
        {
            traits.Add(trait);
            return this;
        }

        public IReferenceItemDeclaration<TItemId> WithoutTrait<TTrait>()
        {
            traits.Remove<TTrait>();
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

        public virtual void Initialize(IEntityViewControl<TItemId> v, TItemId k)
        {
            foreach (var itemTrait in traits)
            {
                itemTrait.Initialize(v, k, this);
            }
        }

        public virtual void Apply(IEntityViewControl<TItemId> v, TItemId k)
        {
            foreach (var itemTrait in traits)
            {
                itemTrait.Apply(v, k, this);
            }
        }
    }
}
