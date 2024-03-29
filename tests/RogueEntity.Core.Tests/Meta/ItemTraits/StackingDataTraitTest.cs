﻿using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Meta.EntityKeys;

namespace RogueEntity.Core.Tests.Meta.ItemTraits
{
    public class StackingDataTraitTest : ItemComponentTraitTestBase<ItemReference, StackCount, StackingBulkTrait<ItemReference>>
    {
        public StackingDataTraitTest() : base(new ItemReferenceMetaData())
        {
        }

        protected override StackingBulkTrait<ItemReference> CreateTrait()
        {
            return new StackingBulkTrait<ItemReference>(1, 100);
        }

        protected override IItemComponentTestDataFactory<StackCount> ProduceTestData(EntityRelations<ItemReference> relations)
        {
            return new ItemComponentTestDataFactory<StackCount>(StackCount.Of(1, 100), 
                                                                StackCount.Of(20, 100), 
                                                                StackCount.Of(100, 100))
                .WithRemoveProhibited();
        }

        [Test]
        public void Adding()
        {
            var newStack = StackCount.Of(1, 50).Add(40, out _);
            newStack.Count.Should().Be(41);
            newStack.MaximumStackSize.Should().Be(50);
        }
    }
}