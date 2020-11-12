using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Sensing.Receptors.Noise
{
    public static class NoiseSenseTraitDeclarations
    {
        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithTouchSense<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                   float sensorStrength, bool enabled = true)
            where TItemId : IEntityKey
        {
            var physics = builder.ServiceResolver.GetOrCreateNoiseSensorPhysics();
            var trait = new NoiseDirectionSenseTrait<TGameContext, TItemId>(physics, sensorStrength, enabled);
            builder.Declaration.WithTrait(trait);
            return builder;
        }

    }
}