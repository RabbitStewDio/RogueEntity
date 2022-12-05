using EnTTSharp;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System.Collections.Generic;

namespace RogueEntity.Core.Meta.ItemTraits;

public class StackingReferenceItemTrait<TItemId>: SimpleReferenceItemComponentTraitBase<TItemId, StackCount> 
    where TItemId : struct, IEntityKey
{
    readonly ushort initialCount;
    readonly ushort stackSize;

    public StackingReferenceItemTrait(ushort stackSize) : this(stackSize, stackSize)
    {
    }

    public StackingReferenceItemTrait(ushort initialCount, ushort stackSize) : base("ItemTrait.Reference.Generic.Stacking", 100)
    {
        this.initialCount = initialCount;
        this.stackSize = stackSize;
    }

    protected override Optional<StackCount> CreateInitialValue(TItemId reference)
    {
        return StackCount.OfRaw(initialCount, stackSize);
    }

    public override IEnumerable<EntityRoleInstance> GetEntityRoles()
    {
        yield return CoreModule.ItemRole.Instantiate<TItemId>();
    }
}