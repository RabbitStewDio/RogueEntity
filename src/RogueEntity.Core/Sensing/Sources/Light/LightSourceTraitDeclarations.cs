using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Sensing.Sources.Light
{
    public static class LightSourceTraitDeclarations
    {
        public static ReferenceItemDeclarationBuilder< TItemId> WithLightSource< TItemId>(this ReferenceItemDeclarationBuilder< TItemId> builder,
                                                                                                                    float intensity, bool enabled = true)
            where TItemId : IEntityKey
        {
            return builder.WithLightSource(0, 0, intensity, enabled);
        }

        public static ReferenceItemDeclarationBuilder< TItemId> WithLightSource< TItemId>(this ReferenceItemDeclarationBuilder< TItemId> builder,
                                                                                                                    float hue, float saturation, float intensity, bool enabled = true)
            where TItemId : IEntityKey
        {
            var lightPhysics = builder.ServiceResolver.Resolve<ILightPhysicsConfiguration>();
            var trait = new LightSourceTrait< TItemId>(lightPhysics, hue, saturation, intensity, enabled);
            builder.Declaration.WithTrait(trait);
            return builder;
        }
    }
}