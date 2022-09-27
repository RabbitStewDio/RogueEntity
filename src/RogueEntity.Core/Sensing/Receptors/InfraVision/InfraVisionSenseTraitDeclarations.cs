using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Sensing.Receptors.InfraVision
{
    public static class InfraVisionSenseTraitDeclarations
    {
        public static ReferenceItemDeclarationBuilder< TItemId> WithInfraVisionSense< TItemId>(this ReferenceItemDeclarationBuilder< TItemId> builder,
                                                                                                                   float receptorIntensity, bool enabled = true)
            where TItemId : struct, IEntityKey
        {
            var physics = builder.ServiceResolver.GetOrCreateInfraVisionSensorPhysics();
            var trait = new InfraVisionSenseTrait< TItemId>(physics, receptorIntensity, enabled);
            builder.Declaration.WithTrait(trait);
            return builder;
        }
    }
}