using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Tests.Meta.Items
{
    public abstract class ItemComponentTraitTestBase<TGameContext, TItemId, TData, TItemTrait>: ItemComponentSerializationTestBase<TGameContext, TItemId, TData>
        where TItemId : IBulkDataStorageKey<TItemId>
        where TItemTrait : IItemComponentTrait<TGameContext, TItemId, TData>
    {
        protected static readonly ItemDeclarationId BulkItemId = "Bulk-TestSubject";
        protected static readonly ItemDeclarationId ReferenceItemId = "Reference-TestSubject";
        
        TGameContext context;

        protected TItemTrait SubjectTrait { get; private set; }
        protected override List<ItemDeclarationId> ActiveItems { get; }
        protected override TGameContext Context => context;

        protected ItemComponentTraitTestBase(IBulkDataStorageMetaData<TItemId> metaData) : base(metaData)
        {
            ActiveItems = new List<ItemDeclarationId>();
            EnableSerializationTest = true;
        }
        
        protected abstract TGameContext CreateContext();
        protected abstract TItemTrait CreateTrait();
        
        [SetUp]
        public void SetUp()
        {
            ActiveItems.Clear();
            base.SetUpItems();
            context = CreateContext();

            EntityRegistry.RegisterNonConstructable<ItemDeclarationHolder<TGameContext, TItemId>>();

            SubjectTrait = CreateTrait();

            if (SubjectTrait is IBulkItemTrait<TGameContext, TItemId> bulkTrait)
            {
                ItemRegistry.Register(CreateBulkItemDeclaration(bulkTrait));
                ActiveItems.Add(BulkItemId);
            }

            if (SubjectTrait is IReferenceItemTrait<TGameContext, TItemId> refTrait)
            {
                ItemRegistry.Register(CreateReferenceItemDeclaration(refTrait));
                ActiveItems.Add(ReferenceItemId);
            }

            if (ActiveItems.Count == 0)
            {
                throw new InvalidOperationException("No valid implementation found. The type given is neither a reference nor a bulk item trait");
            }

            if (!EntityRegistry.IsManaged<TData>())
            {
                EntityRegistry.RegisterNonConstructable<TData>();
            }
        }

        protected virtual BulkItemDeclaration<TGameContext, TItemId> CreateBulkItemDeclaration(IBulkItemTrait<TGameContext, TItemId> bulkTrait)
        {
            return new BulkItemDeclaration<TGameContext, TItemId>(BulkItemId).WithTrait(bulkTrait);
        }

        protected virtual ReferenceItemDeclaration<TGameContext, TItemId> CreateReferenceItemDeclaration(IReferenceItemTrait<TGameContext, TItemId> refTrait)
        {
            return new ReferenceItemDeclaration<TGameContext, TItemId>(ReferenceItemId).WithTrait(refTrait);
        }

        [Test]
        public void Validate_Initialize()
        {
            foreach (var e in ActiveItems)
            {
                Validate_Initialize(e);
            }
        }

        [Test]
        public void Validate_Apply()
        {
            foreach (var e in ActiveItems)
            {
                Validate_Apply(e);
            }
        }

        protected virtual void Validate_Apply(ItemDeclarationId itemId)
        {
            var item = ItemResolver.Instantiate(Context, itemId);
            var testData = ProduceTestData(ProduceItemRelations(item));
            if (testData.UpdateAllowed)
            {
                ItemResolver.TryUpdateData(item, Context, testData.ChangedValue, out item).Should().BeTrue($"because {item} has been successfully updated.");
                ItemResolver.TryQueryData(item, Context, out TData data).Should().BeTrue();
                data.Should().Be(testData.ChangedValue);
            }

            ItemResolver.Apply(item, Context);

            if (testData.TryGetApplyValue(out var applyValue))
            {
                ItemResolver.TryQueryData(item, Context, out TData data2).Should().BeTrue();
                data2.Should().Be(applyValue, "because apply should not reset existing data.");
            }
            else
            {
                ItemResolver.TryQueryData(item, Context, out TData _).Should().BeFalse();
            }
        }

        protected virtual void Validate_Initialize(ItemDeclarationId itemId)
        {
            var item = ItemResolver.Instantiate(Context, itemId);
            var testData = ProduceTestData(ProduceItemRelations(item));

            if (testData.TryGetInitialValue(out var initialData))
            {
                ItemResolver.TryQueryData(item, Context, out TData data).Should().BeTrue();
                data.Should().Be(initialData);
            }
            else
            {
                ItemResolver.TryQueryData(item, Context, out TData _).Should().BeFalse();
            }
        }

        [Test]
        public void Validate_Remove()
        {
            foreach (var e in ActiveItems)
            {
                Validate_Remove(e);
            }
        }

        protected virtual void Validate_Remove(ItemDeclarationId itemId)
        {
            var item = ItemResolver.Instantiate(Context, itemId);
            var testData = ProduceTestData(ProduceItemRelations(item));

            if (!testData.RemoveAllowed)
            {
                // If removing data is not allowed, the contained data should not change

                ItemResolver.TryQueryData(item, Context, out TData beforeRemoved).Should().BeTrue();
                ItemResolver.TryRemoveData<TData>(item, Context, out item).Should().BeFalse();
                ItemResolver.TryQueryData(item, Context, out TData data).Should().BeTrue();
                data.Should().Be(beforeRemoved);
            }
            else if (testData.TryGetRemoved(out var removed))
            {
                // If removing data is allowed, the contained data should be set to a valid value
                ItemResolver.TryRemoveData<TData>(item, Context, out item).Should().BeTrue();
                ItemResolver.TryQueryData(item, Context, out TData data).Should().BeTrue();
                data.Should().Be(removed);
            }
            else
            {
                // If removing data is allowed, the contained data should no longer be present at all.
                ItemResolver.TryRemoveData<TData>(item, Context, out item).Should().BeTrue();
                ItemResolver.TryQueryData(item, Context, out TData _).Should().BeFalse();
            }
        }

        [Test]
        public void Validate_Update()
        {
            foreach (var e in ActiveItems)
            {
                Validate_Update(e);
            }
        }

        protected virtual void Validate_Update(ItemDeclarationId itemId)
        {
            var item = ItemResolver.Instantiate(Context, itemId);
            var testData = ProduceTestData(ProduceItemRelations(item));

            if (testData.UpdateAllowed)
            {
                ItemResolver.TryUpdateData(item, Context, testData.ChangedValue, out item).Should().BeTrue();

                ItemResolver.TryQueryData(item, Context, out TData data).Should().BeTrue();
                data.Should().Be(testData.ChangedValue);

                if (testData.TryGetInvalid(out var invalid))
                {
                    ItemResolver.TryUpdateData(item, Context, invalid, out item).Should().BeFalse();

                    ItemResolver.TryQueryData(item, Context, out TData data2).Should().BeTrue();
                    data2.Should().Be(testData.ChangedValue);
                }
            }
            else
            {
                ItemResolver.TryQueryData(item, Context, out TData beforeUpdate).Should().BeTrue();
                ItemResolver.TryUpdateData(item, Context, testData.ChangedValue, out item).Should().BeFalse();
                ItemResolver.TryQueryData(item, Context, out TData data).Should().BeTrue();
                data.Should().Be(beforeUpdate);
            }
        }

    }
}