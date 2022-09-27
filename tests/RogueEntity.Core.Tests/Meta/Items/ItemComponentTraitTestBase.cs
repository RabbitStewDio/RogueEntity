using EnTTSharp.Entities;
using EnTTSharp.Serialization;
using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Tests.Meta.Items
{
    public abstract class ItemComponentTraitTestBase<TItemId, TData, TItemTrait> : ItemComponentSerializationTestBase<TItemId, TData>
        where TItemId : struct, IBulkDataStorageKey<TItemId>
        where TItemTrait : IItemComponentTrait<TItemId, TData>
    {
        protected static readonly ItemDeclarationId BulkItemId = "Bulk-TestSubject";
        protected static readonly ItemDeclarationId ReferenceItemId = "Reference-TestSubject";

        protected TItemTrait SubjectTrait { get; private set; }
        protected override List<ItemDeclarationId> ActiveItems { get; }

        protected ItemComponentTraitTestBase(IBulkDataStorageMetaData<TItemId> metaData) : base(metaData)
        {
            ActiveItems = new List<ItemDeclarationId>();
            EnableSerializationTest = true;
        }

        protected abstract TItemTrait CreateTrait();

        protected IEntityKeyMapper CreateEntityMapper<TEntityKey>(IBulkDataStorageMetaData<TEntityKey> m)
            where TEntityKey : IEntityKey
        {
            return new DefaultEntityKeyMapper().Register(d => m.CreateReferenceKey(d.Age, d.Key));
        }
        
        [SetUp]
        public void SetUp()
        {
            ActiveItems.Clear();
            base.SetUpItems();
            SetUpPrepare();
            EntityRegistry.RegisterNonConstructable<ItemDeclarationHolder<TItemId>>();

            SubjectTrait = CreateTrait();

            if (SubjectTrait is IBulkItemTrait<TItemId> bulkTrait)
            {
                ItemRegistry.Register(CreateBulkItemDeclaration(bulkTrait));
                ActiveItems.Add(BulkItemId);
            }

            if (SubjectTrait is IReferenceItemTrait<TItemId> refTrait)
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

        protected virtual void SetUpPrepare()
        {
            
        }

        protected virtual IBulkItemDeclaration<TItemId> CreateBulkItemDeclaration(IBulkItemTrait<TItemId> bulkTrait)
        {
            return new BulkItemDeclaration<TItemId>(BulkItemId).WithTrait(bulkTrait);
        }

        protected virtual IReferenceItemDeclaration<TItemId> CreateReferenceItemDeclaration(IReferenceItemTrait<TItemId> refTrait)
        {
            return new ReferenceItemDeclaration<TItemId>(ReferenceItemId).WithTrait(refTrait);
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
            var item = ItemResolver.Instantiate(itemId);
            var testData = ProduceTestData(ProduceItemRelations(item));
            if (testData.UpdateAllowed)
            {
                ItemResolver.TryUpdateData(item, testData.ChangedValue, out item).Should().BeTrue($"because {item} has been successfully updated.");
                ItemResolver.TryQueryData(item, out TData data).Should().BeTrue();
                data.Should().Be(testData.ChangedValue);
            }

            ItemResolver.Apply(item);

            if (testData.TryGetApplyValue(out var applyValue))
            {
                ItemResolver.TryQueryData(item, out TData data2).Should().BeTrue();
                data2.Should().Be(applyValue, "because apply should not reset existing data.");
            }
            else
            {
                ItemResolver.TryQueryData(item, out TData _).Should().BeFalse();
            }
        }

        protected virtual void Validate_Initialize(ItemDeclarationId itemId)
        {
            var item = ItemResolver.Instantiate(itemId);
            var testData = ProduceTestData(ProduceItemRelations(item));

            if (testData.TryGetInitialValue(out var initialData))
            {
                ItemResolver.TryQueryData(item, out TData data).Should().BeTrue();
                data.Should().Be(initialData);
            }
            else
            {
                ItemResolver.TryQueryData(item, out TData _).Should().BeFalse();
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
            var item = ItemResolver.Instantiate(itemId);
            var testData = ProduceTestData(ProduceItemRelations(item));

            if (!testData.RemoveAllowed)
            {
                // If removing data is not allowed, the contained data should not change

                ItemResolver.TryQueryData(item, out TData beforeRemoved).Should().BeTrue();
                ItemResolver.TryRemoveData<TData>(item, out item).Should().BeFalse();
                ItemResolver.TryQueryData(item, out TData data).Should().BeTrue();
                data.Should().Be(beforeRemoved);
            }
            else if (testData.TryGetRemoved(out var removed))
            {
                // If removing data is allowed, the contained data should be set to a valid value
                ItemResolver.TryRemoveData<TData>(item, out item).Should().BeTrue();
                ItemResolver.TryQueryData(item, out TData data).Should().BeTrue();
                data.Should().Be(removed);
            }
            else
            {
                // If removing data is allowed, the contained data should no longer be present at all.
                ItemResolver.TryRemoveData<TData>(item, out item).Should().BeTrue();
                ItemResolver.TryQueryData(item, out TData _).Should().BeFalse();
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
            var item = ItemResolver.Instantiate(itemId);
            var testData = ProduceTestData(ProduceItemRelations(item));

            if (testData.UpdateAllowed)
            {
                ItemResolver.TryUpdateData(item, testData.ChangedValue, out item).Should().BeTrue();

                ItemResolver.TryQueryData(item, out TData data).Should().BeTrue();
                data.Should().Be(testData.ChangedValue);

                if (testData.TryGetInvalid(out var invalid))
                {
                    ItemResolver.TryUpdateData(item, invalid, out item).Should().BeFalse();

                    ItemResolver.TryQueryData(item, out TData data2).Should().BeTrue();
                    data2.Should().Be(testData.ChangedValue);
                }
            }
            else
            {
                ItemResolver.TryQueryData(item, out TData beforeUpdate).Should().BeTrue();
                ItemResolver.TryUpdateData(item, testData.ChangedValue, out item).Should().BeFalse();
                ItemResolver.TryQueryData(item, out TData data).Should().BeTrue();
                data.Should().Be(beforeUpdate);
            }
        }
    }
}
