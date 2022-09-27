using EnTTSharp.Entities;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.MapLoading.FlatLevelMaps
{
    public static class ChangeLevelCommandTraitDeclarations
    {
        public static ReferenceItemDeclarationBuilder<TActorId> WithChangeLevelCommand<TActorId>(this ReferenceItemDeclarationBuilder<TActorId> builder)
            where TActorId : struct, IEntityKey
        {
            builder.WithTrait(CommandInProgressTrait<TActorId>.Instance);
            return builder.WithTrait(new ChangeLevelCommandTrait<TActorId>(builder.ServiceResolver.ResolveToReference<IMapRegionMetaDataService<int>>()));
        }


    }
}
