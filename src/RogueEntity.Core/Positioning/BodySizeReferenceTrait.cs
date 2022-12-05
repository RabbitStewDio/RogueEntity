using EnTTSharp;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System.Collections.Generic;

namespace RogueEntity.Core.Positioning;

public class BodySizeReferenceTrait<TItemId>: SimpleReferenceItemComponentTraitBase<TItemId, BodySize> 
    where TItemId : struct, IEntityKey
{
    readonly BodySize bodySize;

    public BodySizeReferenceTrait(BodySize bodySize) : base("ReferenceItem.Generic.BodySize", 90)
    {
        this.bodySize = bodySize;
    }

    protected override Optional<BodySize> CreateInitialValue(TItemId reference)
    {
        return bodySize;
    }

    public override IEnumerable<EntityRoleInstance> GetEntityRoles()
    {
        yield break;
    }
}