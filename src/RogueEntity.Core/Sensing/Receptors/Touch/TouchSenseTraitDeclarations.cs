using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Sensing.Sources.Touch;

namespace RogueEntity.Core.Sensing.Receptors.Touch
{
    public static class TouchSenseTraitDeclarations
    {
        public static ReferenceItemDeclarationBuilder< TItemId> WithTouchSense< TItemId>(this ReferenceItemDeclarationBuilder< TItemId> builder,
                                                                                                                   bool enabled = true)
            where TItemId : struct, IEntityKey
        {
            var physics = builder.ServiceResolver.GetOrCreateTouchPhysics();
            var trait = new TouchSenseTrait< TItemId>(physics, enabled);
            builder.Declaration.WithTrait(trait);
            return builder;
        }
    }
}