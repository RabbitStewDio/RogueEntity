using System;
using EnTTSharp.Entities;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Tests.Meta.Items
{
    public class ItemDeclarationTest
    {
        BasicItemContext context;

        [SetUp]
        public void SetUp()
        {
            context = new BasicItemContext();
            context.ItemEntities.RegisterNonConstructable<ItemDeclarationHolder<BasicItemContext, ItemReference>>();
        }

        [Test]
        public void Declare_Bulk_Item()
        {
            var reg = context.ItemRegistry;

            var itemDeclaration = new BulkItemDeclaration<BasicItemContext, ItemReference>("bulkitem", "bulkitem.tag");
            reg.Register(itemDeclaration);
            reg.TryGetBulkItemById("bulkitem", out _).Should().BeTrue();
            reg.TryGetItemById("bulkitem", out _).Should().BeTrue();

            var itemId = context.ItemResolver.Instantiate(context, reg.ReferenceItemById("bulkitem"));
            itemId.IsEmpty.Should().BeFalse();
            itemId.IsReference.Should().BeFalse();
            itemId.BulkItemId.Should().Be(1);
            itemId.Data.Should().Be(0);
        }

        [Test]
        public void Declare_Reference_Item()
        {
            var reg = context.ItemRegistry;

            var itemDeclaration = new ReferenceItemDeclaration<BasicItemContext, ItemReference>("refitem", "refitem.tag");
            reg.Register(itemDeclaration);
            reg.TryGetBulkItemById("refitem", out _).Should().BeFalse();
            reg.TryGetItemById("refitem", out _).Should().BeTrue();

            var itemId = context.ItemResolver.Instantiate(context, reg.ReferenceItemById("refitem"));
            itemId.IsEmpty.Should().BeFalse();
            itemId.IsReference.Should().BeTrue();
            itemId.BulkItemId.Should().Be(0);
            itemId.Data.Should().Be(0);
            itemId.Age.Should().Be(1);
            itemId.Key.Should().Be(0);
        }

        [Test]
        public void Instantiating_Invalid_Item_Fails()
        {
            var reg = context.ItemRegistry;

            Action act = () => context.ItemResolver.Instantiate(context, reg.ReferenceItemById("refitem"));
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void ItemReferenceBulk()
        {
            var itemId = ItemReference.FromBulkItem(1, 0);
            itemId.IsEmpty.Should().BeFalse();
            itemId.BulkItemId.Should().Be(1);
            itemId.Data.Should().Be(0);
        }

        [Test]
        public void ReferenceItems_Override_Traits()
        {
            var baseTrait = new CallTracerReferenceTrait("reftrait", 20);
            var overrideTrait = new CallTracerReferenceTrait("reftrait", 10);

            var itemDeclaration = new ReferenceItemDeclaration<BasicItemContext, ItemReference>("refitem", "refitem.tag");
            itemDeclaration.WithTrait(baseTrait);
            itemDeclaration.WithTrait(overrideTrait);

            itemDeclaration.QueryAll<CallTracerReferenceTrait>().Should().BeEquivalentTo(overrideTrait);
        }

        [Test]
        public void BulkItems_Override_Traits()
        {
            var baseTrait = new CallTracerBulkTrait("bulktrait", 20);
            var overrideTrait = new CallTracerBulkTrait("bulktrait", 10);

            var itemDeclaration = new BulkItemDeclaration<BasicItemContext, ItemReference>("bulkitem", "bulkitem.tag");
            itemDeclaration.WithTrait(baseTrait);
            itemDeclaration.WithTrait(overrideTrait);

            itemDeclaration.QueryAll<CallTracerBulkTrait>().Should().BeEquivalentTo(overrideTrait);
        }

        [Test]
        public void BulkItems_Override_Traits_By_Priority()
        {
            var baseTrait = new CallTracerBulkTrait("bulktrait", 20);
            var overrideTrait = new CallTracerBulkTrait("bulktrait", 10);

            var itemDeclaration = new BulkItemDeclaration<BasicItemContext, ItemReference>("bulkitem", "bulkitem.tag");
            itemDeclaration.WithTrait(overrideTrait);
            itemDeclaration.WithTrait(baseTrait);

            itemDeclaration.QueryAll<CallTracerBulkTrait>().Should().BeEquivalentTo(overrideTrait);
        }

        [Test]
        public void ReferenceItems_Call_Init_And_Apply()
        {
            var reg = context.ItemRegistry;
            var itemTrait = new CallTracerReferenceTrait("reftrait", 10);

            var itemDeclaration = new ReferenceItemDeclaration<BasicItemContext, ItemReference>("refitem", "refitem.tag");
            itemDeclaration.WithTrait(itemTrait);
            reg.Register(itemDeclaration);

            var item = context.ItemResolver.Instantiate(context, "refitem");

            itemTrait.InitCallCount.Should().Be(1);
            itemTrait.ApplyCallCount.Should().Be(0);

            context.ItemResolver.Apply(item, context);

            itemTrait.InitCallCount.Should().Be(1);
            itemTrait.ApplyCallCount.Should().Be(1);
        }

        [Test]
        public void BulkItems_Call_Init_And_Apply()
        {
            var reg = context.ItemRegistry;
            var itemTrait = new CallTracerBulkTrait("bulktrait", 10);

            var itemDeclaration = new BulkItemDeclaration<BasicItemContext, ItemReference>("bulkitem", "bulkitem.tag");
            itemDeclaration.WithTrait(itemTrait);
            reg.Register(itemDeclaration);

            var item = context.ItemResolver.Instantiate(context, "bulkitem");

            itemTrait.InitCallCount.Should().Be(1);

            context.ItemResolver.Apply(item, context);

            itemTrait.InitCallCount.Should().Be(1);
        }

        class CallTracerReferenceTrait : IReferenceItemTrait<BasicItemContext, ItemReference>
        {
            public CallTracerReferenceTrait(string id, int priority)
            {
                Id = id;
                Priority = priority;
            }

            public string Id { get; }
            public int Priority { get; }
            public int InitCallCount { get; private set; }
            public int ApplyCallCount { get; private set; }

            public void Initialize(IEntityViewControl<ItemReference> v, BasicItemContext context, ItemReference k, IItemDeclaration item)
            {
                InitCallCount += 1;
            }

            public void Apply(IEntityViewControl<ItemReference> v, BasicItemContext context, ItemReference k, IItemDeclaration item)
            {
                ApplyCallCount += 1;
            }

            public IReferenceItemTrait<BasicItemContext, ItemReference> CreateInstance()
            {
                return new CallTracerReferenceTrait(Id, Priority);
            }
        }

        class CallTracerBulkTrait : IBulkItemTrait<BasicItemContext, ItemReference>
        {
            public CallTracerBulkTrait(string id, int priority)
            {
                Id = id;
                Priority = priority;
            }

            public string Id { get; }
            public int Priority { get; }
            public int InitCallCount { get; private set; }

            public ItemReference Initialize(BasicItemContext context, IItemDeclaration item, ItemReference reference)
            {
                InitCallCount += 1;
                return reference.WithData(Priority);
            }

            public IBulkItemTrait<BasicItemContext, ItemReference> CreateInstance()
            {
                return new CallTracerBulkTrait(Id, Priority);
            }
        }
    }
}