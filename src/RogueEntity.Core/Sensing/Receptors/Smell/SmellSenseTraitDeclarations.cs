using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Sensing.Receptors.Smell
{
    public static class SmellSenseTraitDeclarations
    {
        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithTouchSense<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                   float sensorStrength, bool enabled = true)
            where TItemId : IEntityKey
        {
            var physics = builder.ServiceResolver.GetOrCreateSmellSensorPhysics();
            var trait = new SmellDirectionSenseTrait<TGameContext, TItemId>(physics, sensorStrength, enabled);
            builder.Declaration.WithTrait(trait);
            return builder;
        }

    }
}