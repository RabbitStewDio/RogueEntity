using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Time;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Core.Movement.GridMovement
{
    public class GridMovementModule : ModuleBase
    {
        public const string ModuleId = "Core.Movement.GridMovement";
        public static readonly EntityRole MovementIntentRole = new EntityRole("Role.Core.Movement.GridMovement.MovementIntent");
        public static readonly EntityRole GridMovementCommandReceiverRole = new EntityRole("Role.Core.Movement.GridMovement.MoveCommandReceiver");

        public static readonly EntitySystemId RegisterMovementIntentComponentsId = new EntitySystemId("System.Core.Movement.GridMovement.RegisterMovementIntent");
        public static readonly EntitySystemId ClearMovementIntentSystemId = new EntitySystemId("System.Core.Movement.GridMovement.ClearExpiredMovementIntents");
        public static readonly EntitySystemId RegisterMovementCommandComponentsId = new EntitySystemId("System.Core.Movement.GridMovement.RegisterMovementCommand");
        public static readonly EntitySystemId MovementCommandSystemId = new EntitySystemId("System.Core.Movement.GridMovement.HandleMovementCommand");

        public GridMovementModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Grid Movement";
            Description = "Provides movement commands and UI support";
            IsFrameworkModule = true;

            DeclareDependency(ModuleDependency.Of(CommandsModule.ModuleId));
            DeclareDependency(ModuleDependency.Of(PositionModule.ModuleId));

            RequireRole(MovementIntentRole);
            RequireRole(GridMovementCommandReceiverRole)
                .WithImpliedRole(CommandsModule.CommandExecutionTrackerRole)
                .WithImpliedRole(PositionModule.GridPositionedRole);

            RequireRole(CommandRoles.CreateRoleFor(CommandType.Of<GridMoveCommand>()))
                .WithImpliedRole(GridMovementCommandReceiverRole);
        }

        [EntityRoleInitializer("Role.Core.Movement.GridMovement.MovementIntent")]
        protected void InitializeMoveIntent<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                     IModuleInitializer initializer,
                                                     EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(RegisterMovementIntentComponentsId, -20_000, RegisterMovementIntent);
            entityContext.Register(ClearMovementIntentSystemId, 20_000, RegisterClearMovementIntentSystem);
        }

        void RegisterClearMovementIntentSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter, IGameLoopSystemRegistration context, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var s = new MovementIntentSystem(initParameter.ServiceResolver.ResolveToReference<ITimeSource>());
            var systemRef = registry.BuildSystem().WithoutContext().WithInputParameter<MovementIntent>().CreateSystem(s.ClearMovementIntents);
            context.AddLateFixedStepHandlerSystem(systemRef);
        }

        [EntityRoleInitializer("Role.Core.Movement.GridMovement.MoveCommandReceiver")]
        protected void InitializeMoveCommand<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                      IModuleInitializer initializer,
                                                      EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(RegisterMovementCommandComponentsId, -20_000, RegisterMovementCommand);
            entityContext.Register(MovementCommandSystemId, 21_000, RegisterMovementSystem);
        }

        void RegisterMovementSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter, IGameLoopSystemRegistration context, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var sr = initParameter.ServiceResolver;
            if (!sr.TryResolve(out GridMoveCommandSystem<TItemId> ms))
            {
                ms = new GridMoveCommandSystem<TItemId>(sr.ResolveToReference<ITimeSource>(),
                                                        sr.Resolve<IItemResolver<TItemId>>(),
                                                        sr.Resolve<IMovementDataProvider>(),
                                                        sr.Resolve<IItemPlacementService<TItemId>>());
            }

            var processMovementSystem = registry.BuildSystem()
                                                .WithoutContext()
                                                .WithInputParameter<EntityGridPosition>()
                                                .WithOutputParameter<CommandInProgress>()
                                                .WithOutputParameter<GridMoveCommand>()
                                                .CreateSystem(ms.ProcessMovement);
            context.AddFixedStepHandlerSystem(processMovementSystem);

            var clearMoveActions = registry.BuildSystem()
                                           .WithoutContext()
                                           .WithInputParameter<CommandInProgress>()
                                           .WithInputParameter<GridMoveCommand>()
                                           .CreateSystem(ms.CleanUpMoveAction);
            context.AddFixedStepHandlerSystem(clearMoveActions);
        }


        void RegisterMovementIntent<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<MovementIntent>();
        }

        void RegisterMovementCommand<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<GridMoveCommand>();
        }
    }
}
