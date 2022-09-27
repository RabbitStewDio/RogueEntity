using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Sensing.Receptors.Noise
{
    public static class NoiseSenseTraitDeclarations
    {
        public static ReferenceItemDeclarationBuilder< TItemId> WithTouchSense< TItemId>(this ReferenceItemDeclarationBuilder< TItemId> builder,
                                                                                                                   float sensorStrength, bool enabled = true)
            where TItemId : struct, IEntityKey
        {
            var physics = builder.ServiceResolver.GetOrCreateNoiseSensorPhysics();
            var trait = new NoiseDirectionSenseTrait< TItemId>(physics, sensorStrength, enabled);
            builder.Declaration.WithTrait(trait);
            return builder;
        }

    }
}