using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.Naming;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Discovery;

namespace RogueEntity.Simple.MineSweeper
{
    public static class MineSweeperItemDefinitions
    {
        public static readonly ItemDeclarationId PlayerId = "Player";
        public static readonly ItemDeclarationId Wall = "Items.Wall";
        public static readonly ItemDeclarationId Floor = "Items.Floor";
        public static readonly ItemDeclarationId Mine = "Items.Mine";
        public static readonly ItemDeclarationId Flag = "Items.Flag";

        public static ReferenceItemDeclarationBuilder<TItemId> DefinePlayer<TItemId>(this ItemDeclarationBuilderWithReferenceContext<TItemId> b)
            where TItemId : IEntityKey
        {
            return b.Define(PlayerId)
                    .WithName("player")
                    .WithCommand(CommandType.Of<ToggleFlagCommand>())
                    .WithCommand(CommandType.Of<RevealMapPositionCommand>())
                    .WithDiscoveryMap();
        }
        
        public static BulkItemDeclarationBuilder<TItemId> DefineWall<TItemId>(this ItemDeclarationBuilderWithBulkContext<TItemId> b)
            where TItemId : IEntityKey
        {
            return b.Define(Wall)
                    .WithGridPosition(MineSweeperMapLayers.Items)
                    .AsImmobile()
                    .WithName("wall");
        }

        public static BulkItemDeclarationBuilder<TItemId> DefineFloor<TItemId>(this ItemDeclarationBuilderWithBulkContext<TItemId> b)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return b.Define(Floor)
                    .WithGridPosition(MineSweeperMapLayers.Items)
                    .AsImmobile()
                    .WithTrait(new MineSweeperMineCountItemTrait<TItemId>())
                    .WithName("floor");
        }

        public static BulkItemDeclarationBuilder<TItemId> DefineFlag<TItemId>(this ItemDeclarationBuilderWithBulkContext<TItemId> b)
            where TItemId : IEntityKey
        {
            return b.Define(Flag)
                    .WithGridPosition(MineSweeperMapLayers.Flags)
                    .AsImmobile()
                    .WithRole(MineSweeperModule.MineFieldRole)
                    .WithName("flag");
        }

        public static ReferenceItemDeclarationBuilder<TItemId> DefineMine<TItemId>(this ItemDeclarationBuilderWithReferenceContext<TItemId> b)
            where TItemId : IEntityKey
        {
            return b.Define(Mine)
                    .WithGridPosition(MineSweeperMapLayers.Items)
                    .AsImmobile()
                    .WithRole(MineSweeperModule.MineFieldRole)
                    .WithName("mine");
        }
    }
}
