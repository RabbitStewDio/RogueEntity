using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.MovementPlaning.Goals
{
    public static class GoalMarkerDeclarations
    {
        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithGoalMarker<TGameContext, TItemId, TDiscriminator>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder, GoalMarker<TDiscriminator> g)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new GoalMarkerTrait<TGameContext,TItemId,TDiscriminator>(g));
            return builder;
        }
    }
}