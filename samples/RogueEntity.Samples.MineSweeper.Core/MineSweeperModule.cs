﻿using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.MapLoading.Builder;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Discovery;
using RogueEntity.Samples.MineSweeper.Core.Commands;
using RogueEntity.Samples.MineSweeper.Core.Services;
using RogueEntity.Samples.MineSweeper.Core.Traits;

namespace RogueEntity.Samples.MineSweeper.Core
{
    [Module("MineSweeper")]
    public partial class MineSweeperModule : ModuleBase
    {
        static readonly EntitySystemId RegisterEntitiesSystemId = new EntitySystemId("Entities.MineSweeper.Player");
        static readonly EntitySystemId ProcessCommandsSystemId = new EntitySystemId("System.MineSweeper.ProcessCommands");
        static readonly EntitySystemId GenerateMapSystemId = new EntitySystemId("System.MineSweeper.GenerateMap");

        public static readonly EntityRole MineFloorRole = new EntityRole("Role.MineSweeper.MineFloor");
        public static readonly EntityRole MineFlagRole = new EntityRole("Role.MineSweeper.MineFlag");
        public static readonly EntityRole MineRole = new EntityRole("Role.MineSweeper.Mine");

        static readonly EntityRelation PlayerRevealsFieldRelation = 
            new EntityRelation("Relation.MineSweeper.PlayerRevealsField", PlayerModule.PlayerRole, MineFloorRole);

        public MineSweeperModule()
        {
            Id = "MineSweeper.Game";

            RequireRelation(PlayerRevealsFieldRelation);
        }

        [EntityRoleInitializer("Role.Core.Player")]
        public void InitializePlayer<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                               IModuleInitializer initializer,
                                               EntityRole r)
            where TActorId : struct, IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TActorId>();
            ctx.Register(RegisterEntitiesSystemId, 40_000, RegisterPlayerEntities);
            ctx.Register(ProcessCommandsSystemId, 10_000, RegisterProcessCommandsSystem);
            ctx.Register(GenerateMapSystemId, 10_000, RegisterGenerateMapSystem);
        }

        void RegisterGenerateMapSystem<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                 IGameLoopSystemRegistration context,
                                                 EntityRegistry<TActorId> registry)
            where TActorId : struct, IEntityKey
        {
            var sr = initParameter.ServiceResolver;
            var generator = new MineSweeperMapGenerator<ItemReference>(sr.Resolve<IEntityRandomGeneratorSource>(),
                                                                       sr.Resolve<IItemPlacementServiceContext<ItemReference>>(),
                                                                       sr.Resolve<IMapStateController>(),
                                                                       sr.Resolve<IItemResolver<ItemReference>>(),
                                                                       sr.Resolve<MapBuilder>(),
                                                                       sr.Resolve<IMineSweeperGameParameterService>());
            
            context.AddInitializationStepHandler(generator.Activate);
        }

        void RegisterProcessCommandsSystem<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                     IGameLoopSystemRegistration context,
                                                     EntityRegistry<TActorId> registry)
            where TActorId : struct, IEntityKey
        {
            var sr = initParameter.ServiceResolver;
            var sysMapReveal = registry.BuildSystem()
                                       .WithoutContext()
                                       .WithInputParameter<DiscoveryMapData>()
                                       .WithInputParameter<RevealMapPositionCommand>()
                                       .WithOutputParameter<MineSweeperPlayerData>()
                                       .CreateSystem(new MineSweeperMapRevealSystem<ItemReference>(
                                                         sr.Resolve<IItemPlacementServiceContext<ItemReference>>(),
                                                         sr.Resolve<IItemResolver<ItemReference>>(),
                                                         sr.Resolve<IMineSweeperGameParameterService>()).ProcessInputCommand);

            var sysToggleFlag = registry.BuildSystem()
                                        .WithoutContext()
                                        .WithInputParameter<MineSweeperPlayerData>()
                                        .WithInputParameter<ToggleFlagCommand>()
                                        .CreateSystem(new MineSweeperToggleFlagSystem<ItemReference>(
                                                          sr.Resolve<IItemPlacementServiceContext<ItemReference>>(),
                                                          sr.Resolve<IItemResolver<ItemReference>>(),
                                                          sr.Resolve<MapBuilder>()
                                                      ).ProcessInputCommand);

            context.AddVariableStepHandlerSystem(sysMapReveal);
            context.AddVariableStepHandlerSystem(sysToggleFlag);
        }

        void RegisterPlayerEntities<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter, EntityRegistry<TActorId> registry)
            where TActorId : struct, IEntityKey
        {
            registry.RegisterNonConstructable<RevealMapPositionCommand>();
            registry.RegisterNonConstructable<ToggleFlagCommand>();
            registry.RegisterNonConstructable<MineSweeperPlayerData>();
        }
    }
}
