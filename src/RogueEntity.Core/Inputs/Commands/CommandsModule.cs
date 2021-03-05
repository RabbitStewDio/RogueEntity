using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.Players;

namespace RogueEntity.Core.Inputs.Commands
{
    public static class CommandRoles
    {
        public static EntityRole CreateRoleFor<TCommand>(CommandType<TCommand> cmd)
        {
            return new EntityRole("Role.Core.Inputs.Commands.Executor[]");
        }
    }

    public class CommandsModule : ModuleBase
    {
        public const string ModuleId = "Core.Inputs.Commands";

        public static readonly EntityRole CommandExecutionTrackerRole = new EntityRole("Role.Core.Inputs.Commands.ExecutionTracker");

        public static readonly EntitySystemId RegisterEntitiesSystemId = new EntitySystemId("Entities.Core.Inputs.Commands");
        public static readonly EntitySystemId ClearCommandsSystemId = new EntitySystemId("Systems.Core.Inputs.Commands.ClearExpiredCommands");

        public CommandsModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Commands";
            Description = "Provides base classes and behaviours for submitting and handling commands to player observers";
            IsFrameworkModule = true;

            RequireRole(CommandExecutionTrackerRole);

            DeclareDependency(ModuleDependency.Of(PlayerModule.ModuleId));
        }

        [EntityRoleInitializer("Role.Core.Inputs.Commands.ExecutionTracker")]
        protected void InitializePlayerObserverRole<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                              IModuleInitializer initializer,
                                                              EntityRole r)
            where TActorId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register(RegisterEntitiesSystemId, 20_000, RegisterCommandComponents);
            entityContext.Register(RegisterEntitiesSystemId, 1_000_000, RegisterClearExpiredCommandsSystem);
        }

        void RegisterClearExpiredCommandsSystem<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                          IGameLoopSystemRegistration context,
                                                          EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            var system = new BasicCommandSystem<TActorId>(initParameter.ServiceResolver.Resolve<IItemResolver<TActorId>>());
            var clearCommandsSystem = registry.BuildSystem()
                                              .WithoutContext()
                                              .WithInputParameter<CommandInProgress>()
                                              .CreateSystem(system.ClearHandledCommands);
            context.AddFixedStepHandlerSystem(clearCommandsSystem);
            context.AddVariableStepHandlerSystem(clearCommandsSystem);
        }

        void RegisterCommandComponents<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<CommandInProgress>();
        }
    }
}
