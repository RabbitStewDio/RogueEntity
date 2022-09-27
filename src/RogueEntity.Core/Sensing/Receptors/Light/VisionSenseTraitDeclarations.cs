using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Sensing.Receptors.Light
{
    public static class VisionSenseTraitDeclarations
    {
        public static ReferenceItemDeclarationBuilder< TItemId> WithVisionSense< TItemId>(this ReferenceItemDeclarationBuilder< TItemId> builder,
                                                                                                                         float receptorIntensity, bool enabled = true)
            where TItemId : struct, IEntityKey
        {
            var physics = builder.ServiceResolver.GetOrCreateVisionSensorPhysics();
            var trait = new VisionSenseTrait< TItemId>(physics, receptorIntensity, enabled);
            builder.Declaration.WithTrait(trait);
            return builder;
        }

    }
}