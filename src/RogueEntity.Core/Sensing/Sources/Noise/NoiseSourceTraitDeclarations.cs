using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Sensing.Sources.Noise
{
    public static class NoiseSourceTraitDeclarations
    {
        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithNoiseSource<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder)
            where TItemId : IEntityKey
        {
            var physics = builder.ServiceResolver.Resolve<INoisePhysicsConfiguration>();
            var trait = new NoiseSourceTrait<TGameContext, TItemId>(physics);
            builder.Declaration.WithTrait(trait);
            return builder;
        }

    }
}