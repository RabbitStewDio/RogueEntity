using EnTTSharp.Entities;
using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.EntityKeys;

namespace RogueEntity.Core.Tests.Meta.Items
{
    public abstract class ReferenceItemComponentTraitTestBase<TItemId, TData, TItemTrait>
        where TItemId : struct, IBulkDataStorageKey<TItemId>
        where TItemTrait: IReferenceItemTrait<TItemId>, IItemComponentTrait<TItemId, TData>
    {
        protected static readonly ItemDeclarationId ItemId = "TestSubject";

        protected TItemTrait subjectTrait;

        [SetUp]
        public void SetUp()
        {
            SetUpPrepare();

            EntityRegistry.RegisterNonConstructable<IReferenceItemDeclaration<ItemReference>>();

            subjectTrait = CreateTrait();
            
            ItemRegistry.Register(new ReferenceItemDeclaration<TItemId>(ItemId).WithTrait(subjectTrait));

            if (!EntityRegistry.IsManaged<TData>())
            {
                EntityRegistry.RegisterNonConstructable<TData>();
            }
        }

        protected virtual void SetUpPrepare()
        {
        }

        protected abstract EntityRegistry<TItemId> EntityRegistry { get; }
        protected abstract ItemRegistry<TItemId> ItemRegistry { get; }

        protected abstract TItemTrait CreateTrait();
    }
}