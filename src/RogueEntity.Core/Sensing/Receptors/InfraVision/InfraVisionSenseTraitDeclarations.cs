using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Sensing.Receptors.InfraVision
{
    public static class InfraVisionSenseTraitDeclarations
    {
        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithInfraVisionSense<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                   float receptorIntensity, bool enabled = true)
            where TItemId : IEntityKey
        {
            var physics = builder.ServiceResolver.GetOrCreateInfraVisionSensorPhysics();
            var trait = new InfraVisionSenseTrait<TGameContext, TItemId>(physics, receptorIntensity, enabled);
            builder.Declaration.WithTrait(trait);
            return builder;
        }
    }
}