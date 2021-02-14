using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Discovery;
using RogueEntity.Generator;

namespace RogueEntity.Simple.MineSweeper
{
    [Module("MineSweeper")]
    public partial class MineSweeperModule : ModuleBase
    {
        static readonly EntitySystemId RegisterEntitiesSystemId = new EntitySystemId("Entities.MineSweeper.Player");
        static readonly EntitySystemId ProcessCommandsSystemId = new EntitySystemId("System.ProcessCommands");

        public static readonly EntityRole MineFieldRole = new EntityRole("Role.MineSweeper.MineField");

        static readonly EntityRelation PlayerRevealsFieldRelation = new EntityRelation("Relation.MineSweeper.PlayerRevealsField", PlayerModule.PlayerRole, MineFieldRole);

        public MineSweeperModule()
        {
            Id = "MineSweeper.Game";

            DeclareRelation<ActorReference, ItemReference>(PlayerRevealsFieldRelation);
        }

        [EntityRoleInitializer("Role.Core.Player")]
        public void InitializePlayer<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                               IModuleInitializer initializer,
                                               EntityRole r)
            where TActorId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TActorId>();
            ctx.Register(RegisterEntitiesSystemId, 40_000, RegisterPlayerEntities);
            ctx.Register(ProcessCommandsSystemId, 10_000, RegisterProcessCommandsSystem);
        }

        void RegisterProcessCommandsSystem<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                     IGameLoopSystemRegistration context,
                                                     EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            var sr = initParameter.ServiceResolver;
            var sysMapReveal = registry.BuildSystem()
                                       .WithoutContext()
                                       .WithInputParameter<MineSweeperPlayerData>()
                                       .WithInputParameter<DiscoveryMapData>()
                                       .WithInputParameter<RevealMapPositionCommand>()
                                       .CreateSystem(new MineSweeperMapRevealSystem<ItemReference>(
                                                         sr.Resolve<IGridMapContext<ItemReference>>(),
                                                         sr.Resolve<IItemResolver<ItemReference>>()).ProcessInputCommand);

            var sysToggleFlag = registry.BuildSystem()
                              .WithoutContext()
                              .WithInputParameter<MineSweeperPlayerData>()
                              .WithInputParameter<ToggleFlagCommand>()
                              .CreateSystem(new MineSweeperToggleFlagSystem<ItemReference>(
                                                sr.Resolve<IGridMapContext<ItemReference>>(),
                                                sr.Resolve<IItemResolver<ItemReference>>(),
                                                sr.Resolve<MapBuilder>()
                                            ).ProcessInputCommand);

            context.AddVariableStepHandlers(sysMapReveal, nameof(MineSweeperMapRevealSystem<ItemReference>.ProcessInputCommand));
            context.AddVariableStepHandlers(sysToggleFlag, nameof(MineSweeperToggleFlagSystem<ItemReference>.ProcessInputCommand));
        }

        void RegisterPlayerEntities<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter, EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            registry.RegisterNonConstructable<RevealMapPositionCommand>();
            registry.RegisterNonConstructable<ToggleFlagCommand>();
            registry.RegisterNonConstructable<MineSweeperPlayerData>();
            registry.RegisterNonConstructable<MineSweeperPlayerProfile>();
        }
    }
}
