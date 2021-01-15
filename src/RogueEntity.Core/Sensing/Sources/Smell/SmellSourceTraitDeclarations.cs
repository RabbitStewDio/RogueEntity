using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Sensing.Sources.Smell
{
    public static class SmellSourceTraitDeclarations
    {
        public static ReferenceItemDeclarationBuilder< TItemId> WithNoiseSource< TItemId>(this ReferenceItemDeclarationBuilder< TItemId> builder)
            where TItemId : IEntityKey
        {
            var physics = builder.ServiceResolver.Resolve<ISmellPhysicsConfiguration>();
            var trait = new SmellSourceTrait< TItemId>(physics);
            builder.Declaration.WithTrait(trait);
            return builder;
        }


    }
}