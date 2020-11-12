using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Sensing.Receptors.Heat
{
    public static class HeatDirectionSenseTraitDeclarations
    {
        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithHeatSense<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                   float receptorIntensity, bool enabled = true)
            where TItemId : IEntityKey
        {
            var physics = builder.ServiceResolver.GetOrCreateHeatSensorPhysics();
            var trait = new HeatDirectionSenseTrait<TGameContext, TItemId>(physics, receptorIntensity, enabled);
            builder.Declaration.WithTrait(trait);
            return builder;
        }

    }
}