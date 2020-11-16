using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Meta.ItemTraits;

namespace RogueEntity.Core.Sensing.Sources.Heat
{
    public static class HeatSourceTraitDeclarations
    {
        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithHeatSource<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                   Optional<Temperature> temperature)
            where TItemId : IEntityKey
        {
            var physics = builder.ServiceResolver.Resolve<IHeatPhysicsConfiguration>();
            var trait = new HeatSourceTrait<TGameContext, TItemId>(physics, temperature);
            builder.Declaration.WithTrait(trait);
            return builder;
        }
    }
}