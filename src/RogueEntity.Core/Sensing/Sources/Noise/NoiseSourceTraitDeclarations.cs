using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Sensing.Sources.Noise
{
    public static class NoiseSourceTraitDeclarations
    {
        public static ReferenceItemDeclarationBuilder< TItemId> WithNoiseSource< TItemId>(this ReferenceItemDeclarationBuilder< TItemId> builder)
            where TItemId : struct, IEntityKey
        {
            var physics = builder.ServiceResolver.Resolve<INoisePhysicsConfiguration>();
            var trait = new NoiseSourceTrait< TItemId>(physics);
            builder.Declaration.WithTrait(trait);
            return builder;
        }

    }
}