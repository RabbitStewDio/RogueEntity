using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Sensing.Receptors.Heat
{
    public static class HeatDirectionSenseTraitDeclarations
    {
        public static ReferenceItemDeclarationBuilder<TItemId> WithHeatSense<TItemId>(this ReferenceItemDeclarationBuilder<TItemId> builder,
                                                                                      float receptorIntensity,
                                                                                      bool enabled = true)
            where TItemId : IEntityKey
        {
            var physics = builder.ServiceResolver.GetOrCreateHeatSensorPhysics();
            var trait = new HeatDirectionSenseTrait<TItemId>(physics, receptorIntensity, enabled);
            builder.Declaration.WithTrait(trait);
            return builder;
        }
    }
}
