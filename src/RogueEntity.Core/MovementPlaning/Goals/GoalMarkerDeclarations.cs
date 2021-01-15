using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.MovementPlaning.Goals
{
    public static class GoalMarkerDeclarations
    {
        public static ReferenceItemDeclarationBuilder< TItemId> WithGoalMarker< TItemId, TDiscriminator>(this ReferenceItemDeclarationBuilder< TItemId> builder, GoalMarker<TDiscriminator> g)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new GoalMarkerTrait<TItemId,TDiscriminator>(g));
            return builder;
        }
    }
}