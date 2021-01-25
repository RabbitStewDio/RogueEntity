using FluentAssertions;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Equipment;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Tests.Fixtures;

namespace RogueEntity.Core.Tests.Equipment
{
    public static class SlottedEquipmentFixtureExtensions
    {
        public static ItemReference InstantiatedAsEquipment<TItemFixture>(this EntityContext<TItemFixture> t, Optional<EquipmentSlot> slot)
            where TItemFixture : SlottedEquipmentTest
        {
            t.Context.Equipment.TryEquipItem(t.Item, out _, slot, out var actualSlot).Should().BeTrue();
            if (slot.TryGetValue(out var expectedSlot))
            {
                actualSlot.Should().Be(expectedSlot);
            }

            return t.Item;
        } 
    }
}
