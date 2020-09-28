using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.CacheControl;
using RogueEntity.Core.Sensing.Maps;
using RogueEntity.Core.Sensing.Vision;

namespace RogueEntity.Core.Sensing
{
    public class SensingModule<TGameContext> : ModuleBase<TGameContext>
        where TGameContext : IMapCacheControlProvider, IMapBoundsContext, ISenseContextProvider
    {
        public SensingModule()
        { 
            Id = "Core.Senses";
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Senses";
            Description = "Provides base classes and behaviours for adding senses and vision";
        }

        protected void RegisterAll<TItemId>(TGameContext context, IModuleInitializer<TGameContext> initializer)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register("Core.Entities.LightSources", -19_000, RegisterLightSourceEntities);
            entityContext.Register("Core.Entities.ActorVision", -19_000, RegisterActorVisionEntities);
            entityContext.Register("Core.System.SenseMap.UpdateCacheStatus", 50_000, RegisterClearSenseCacheSystems<TItemId>);
            entityContext.Register("Core.System.SenseMap.Vision", 59_000, RegisterVisibilitySystem<TItemId>);
        }

        void RegisterClearSenseCacheSystems<TItemId>(IGameLoopSystemRegistration<TGameContext> context,
                                                     EntityRegistry<TItemId> registry,
                                                     ICommandHandlerRegistration<TGameContext, TItemId> handler) 
            where TItemId : IEntityKey
        {
            var entitySystemBuilder = registry.BuildSystem().WithContext<TGameContext>();
            context.AddFixedStepHandlers(entitySystemBuilder.CreateSystem<EntityGridPositionChangedMarker>(SenseMappingSystems.ApplyGridMovementChangeTracker));
            context.AddFixedStepHandlers(entitySystemBuilder.CreateSystem<ContinuousMapPositionChangedMarker>(SenseMappingSystems.ApplyContinuousMovementChangeTracker));
        }

        void RegisterVisibilitySystem<TItemId>(IGameLoopSystemRegistration<TGameContext> context,
                                               EntityRegistry<TItemId> registry,
                                               ICommandHandlerRegistration<TGameContext, TItemId> handler) 
            where TItemId : IEntityKey
        {
            var entitySystemBuilder = registry.BuildSystem().WithContext<TGameContext>();
            context.AddFixedStepHandlers(entitySystemBuilder.CreateSystem<EntityGridPosition, VisibilityDetector<TGameContext, TItemId>>(VisibilitySystem<TGameContext, TItemId>.DoUpdateLocalSenseMapGrid));
            context.AddFixedStepHandlers(entitySystemBuilder.CreateSystem<ContinuousMapPosition, VisibilityDetector<TGameContext, TItemId>>(VisibilitySystem<TGameContext, TItemId>.DoUpdateLocalSenseMapContinuous));
        }

        protected void RegisterLightSourceEntities<TItemId>(EntityRegistry<TItemId> registry) where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<LightSourceData>();
        }

        protected void RegisterActorVisionEntities<TActorId>(EntityRegistry<TActorId> registry) where TActorId : IEntityKey
        {
            registry.RegisterNonConstructable<VisibilityDetector<TGameContext, TActorId>>();
            registry.RegisterNonConstructable<DiscoveryMapData>();
        }
    }
}