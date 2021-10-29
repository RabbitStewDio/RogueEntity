using EnTTSharp.Entities;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Generator.Commands
{
    public static class ChangeLevelCommandTraitDeclarations
    {
        public static ReferenceItemDeclarationBuilder<TActorId> WithChangeLevelCommand<TActorId>(this ReferenceItemDeclarationBuilder<TActorId> builder)
            where TActorId : IEntityKey
        {
            builder.WithTrait(CommandInProgressTrait<TActorId>.Instance);
            return builder.WithTrait(new ChangeLevelCommandTrait<TActorId>(builder.ServiceResolver.ResolveToReference<IMapLevelMetaDataService>()));
        }


    }
}
