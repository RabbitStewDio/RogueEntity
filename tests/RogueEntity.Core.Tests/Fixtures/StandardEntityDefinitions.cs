using RogueEntity.Api.Modules;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Core.MapLoading.PlayerSpawning;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.Naming;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.Movement.GridMovement;
using RogueEntity.Core.Movement.MovementModes.Walking;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using static RogueEntity.Core.Movement.CostModifier.MovementCostModifiers;

namespace RogueEntity.Core.Tests.Fixtures
{
    public static class StandardEntityDefinitions
    {
        public static readonly ItemDeclarationInfo Player = ItemDeclarationInfo.Of("Player", "Tag.Actors.Player");
        public static readonly ItemDeclarationInfo SpawnPointFloor = ItemDeclarationInfo.Of("SpawnPoint", "Tag.Ground.SpawnPoint");
        public static readonly ItemDeclarationInfo EmptyFloor = ItemDeclarationInfo.Of("Floor", "Tag.Ground.Floor");
        public static readonly ItemDeclarationInfo Wall = ItemDeclarationInfo.Of("Wall", "Tag.Items.Wall");

        public static void DeclareWalls(ModuleInitializationParameter mp, IModuleInitializer ctx)
        {
            var itemContext = ctx.DeclareContentContext<ItemReference>()
                                 .EnsureEntityRegistered(mp.ServiceResolver);
            itemContext.Activate(itemContext.CreateBulkEntityBuilder(mp.ServiceResolver)
                                            .Define(Wall)
                                            .WithGridPosition(BodySize.OneByOne, TestMapLayers.Ground)
                                            .AsImmobile()
                                            .WithMovementCostModifier(Blocked<WalkingMovement>())
                                            .WithName("wall")
                                            .Declaration);
            itemContext.Activate(itemContext.CreateBulkEntityBuilder(mp.ServiceResolver)
                                            .Define(EmptyFloor)
                                            .WithGridPosition(BodySize.OneByOne, TestMapLayers.Ground)
                                            .AsImmobile()
                                            .WithMovementCostModifier(For<WalkingMovement>(1))
                                            .WithName("floor")
                                            .Declaration);
        }

        public static void DeclareSpawnPoint(ModuleInitializationParameter mp, IModuleInitializer ctx)
        {
            var itemContext = ctx.DeclareContentContext<ItemReference>()
                                 .EnsureEntityRegistered(mp.ServiceResolver);
            itemContext.Activate(itemContext.CreateReferenceEntityBuilder(mp.ServiceResolver)
                                            .Define(SpawnPointFloor)
                                            .WithGridPosition(BodySize.OneByOne, TestMapLayers.Ground)
                                            .AsImmobile()
                                            .AsSpawnLocation()
                                            .WithMovementCostModifier(For<WalkingMovement>(1))
                                            .WithName("floor")
                                            .Declaration);
        }


        public static void DeclarePlayer(ModuleInitializationParameter mp, IModuleInitializer ctx)
        {
            var actorContext = ctx.DeclareContentContext<ActorReference>()
                                  .EnsureEntityRegistered(mp.ServiceResolver);
            actorContext.Activate(actorContext.CreateReferenceEntityBuilder(mp.ServiceResolver)
                                              .Define(Player)
                                              .AsPlayer()
                                              .AsAvatar()
                                              .WithGridPosition(BodySize.OneByOne, TestMapLayers.Actors)
                                              .WithMovement()
                                              .AsPointCost(WalkingMovement.Instance, DistanceCalculation.Euclid, 1)
                                              .WithCommand(CommandType.Of<GridMoveCommand>())
                                              .Declaration);
        }
    }
}
