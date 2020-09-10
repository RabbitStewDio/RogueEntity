using EnTTSharp.Entities;
using NUnit.Framework;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Tests.Meta.Items
{
    public abstract class ReferenceItemComponentTraitTestBase<TGameContext, TItemId, TData, TItemTrait>
        where TItemId : IBulkDataStorageKey<TItemId>
        where TItemTrait: IReferenceItemTrait<TGameContext, TItemId>, IItemComponentTrait<TGameContext, TItemId, TData>
    {
        protected static readonly ItemDeclarationId ItemId = "TestSubject";

        protected TGameContext Context { get; private set; }
        protected TItemTrait subjectTrait;

        [SetUp]
        public void SetUp()
        {
            Context = CreateContext();

            EntityRegistry.RegisterNonConstructable<IReferenceItemDeclaration<BasicItemContext, ItemReference>>();

            subjectTrait = CreateTrait();
            
            ItemRegistry.Register(new ReferenceItemDeclaration<TGameContext, TItemId>(ItemId).WithTrait(subjectTrait));

            if (!EntityRegistry.IsManaged<TData>())
            {
                EntityRegistry.RegisterNonConstructable<TData>();
            }
        }

        protected abstract EntityRegistry<TItemId> EntityRegistry { get; }
        protected abstract ItemRegistry<TGameContext, TItemId> ItemRegistry { get; }

        protected abstract TGameContext CreateContext();
        protected abstract TItemTrait CreateTrait();
    }
}