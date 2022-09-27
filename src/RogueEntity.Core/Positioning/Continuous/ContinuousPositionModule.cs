using EnTTSharp.Entities;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.Meta;

namespace RogueEntity.Core.Positioning.Continuous
{
    public class ContinuousPositionModule: ModuleBase
    {
        public static readonly string ModuleId = "Core.Position.Continuous";
        public static readonly EntityRole ContinuousPositionedRole = new EntityRole("Role.Core.Position.ContinuousPositioned");
        public static readonly EntitySystemId RegisterContinuousPositions = "Entities.Core.Position.Continuous";
        public static readonly EntitySystemId RegisterClearContinuousPositionChangeTracker = "Systems.Core.Position.Continuous.ClearChangeTracker";

        public ContinuousPositionModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core.Continuous";
            Name = "RogueEntity Core Module - Continuous Positioning";
            Description = "Provides support for positioning items in a continuous coordinate system";
            IsFrameworkModule = true;

            RequireRole(ContinuousPositionedRole).WithImpliedRole(PositionModule.PositionedRole).WithDependencyOn(CoreModule.ModuleId).WithDependencyOn(PositionModule.ModuleId);
        }

        [EntityRoleInitializer("Role.Core.Position.ContinuousPositioned")]
        protected void InitializeContinuousPositioned<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                                IModuleInitializer initializer,
                                                                EntityRole role)
            where TActorId : struct, IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register(RegisterContinuousPositions, 0, RegisterContinuousEntities);
            entityContext.Register(RegisterClearContinuousPositionChangeTracker, 10, RegisterClearContinuousPositionChangeTrackers);
        }

        void RegisterContinuousEntities<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                  EntityRegistry<TActorId> registry)
            where TActorId : struct, IEntityKey
        {
            registry.RegisterNonConstructable<ContinuousMapPosition>();
            registry.RegisterNonConstructable<ContinuousMapPositionChangedMarker>();
        }

        void RegisterClearContinuousPositionChangeTrackers<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                                     IGameLoopSystemRegistration context,
                                                                     EntityRegistry<TActorId> registry)
            where TActorId : struct, IEntityKey
        {
            void ClearContinuousPositionAction()
            {
                registry.ResetComponent<ContinuousMapPositionChangedMarker>();
            }

            context.AddFixedStepHandlers(ClearContinuousPositionAction);
            ContinuousMapPositionChangedMarker.InstallChangeHandler(registry);
        }

    }
}
