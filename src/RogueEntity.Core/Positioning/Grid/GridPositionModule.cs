using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.Meta;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.MapLayers;
using System.Linq;

namespace RogueEntity.Core.Positioning.Grid
{
    public class GridPositionModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.Position.Grid";

        public static readonly EntitySystemId RegisterGridPositionsEntityId = "Entities.Core.Position.Grid";
        public static readonly EntitySystemId ClearGridPositionChangeTrackerSystemId = "Systems.Core.Position.Grid.ClearChangeTracker";
        public static readonly EntitySystemId ClearDestroyedEntitiesSystemId = "Systems.Core.Position.Grid.ClearDestroyedEntities";
        public static readonly EntitySystemId RegisterMapDataAggregateSystemId = "Systems.Core.Position.Grid.RegisterMapDataAggregate";
        public static readonly EntityRole GridPositionedRole = new EntityRole("Role.Core.Position.GridPositioned");

        public GridPositionModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Positioning for Grids";
            Description = "Provides support for positioning items in a grid coordinate system";
            IsFrameworkModule = true;


            RequireRole(GridPositionedRole)
                .WithImpliedRole(PositionModule.PositionedRole)
                .WithDependencyOn(CoreModule.ModuleId)
                .WithDependencyOn(PositionModule.ModuleId);
        }

        [EntityRoleInitializer("Role.Core.Position.GridPositioned")]
        protected void InitializeGridPositioned<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                          IModuleInitializer initializer,
                                                          EntityRole role)
            where TActorId : IEntityKey
        {
            EnsureDefaultGridMapExists(initParameter, initializer, role);

            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register(RegisterGridPositionsEntityId, 0, RegisterGridEntities);
            entityContext.Register(RegisterMapDataAggregateSystemId, 0, RegisterMapDataAggregate);
            entityContext.Register(ClearGridPositionChangeTrackerSystemId, 10, RegisterClearGridPositionChangeTrackers);
            entityContext.Register(ClearDestroyedEntitiesSystemId, 110_000, RegisterClearDestroyedEntities);
        }

        /// <summary>
        ///   Ensures that if an entity declares to be grid positioned, that the system contains a
        ///   map with a valid layer for such entities. This method will do nothing if an map system
        ///   already exists.
        /// </summary>
        /// <param name="initParameter"></param>
        /// <param name="initializer"></param>
        /// <param name="role"></param>
        /// <typeparam name="TActorId"></typeparam>
        protected void EnsureDefaultGridMapExists<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                            IModuleInitializer initializer,
                                                            EntityRole role)
            where TActorId : IEntityKey
        {
            if (!initParameter.ServiceResolver.TryResolve<IItemResolver<TActorId>>(out var itemResolver))
            {
                return;
            }

            var mapLayers = itemResolver.ItemRegistry.QueryDesignTimeTrait<MapLayerPreference>()
                                        .SelectMany(e => e.Item2.AcceptableLayers)
                                        .Distinct()
                                        .ToList();
            if (mapLayers.Count == 0)
            {
                return;
            }

            if (!GridPositionModuleServices.TryGetOrCreateDefaultMapServices<TActorId>(initParameter.ServiceResolver, out var map, out var placementService))
            {
                return;
            }

            var itemPlacementService = initParameter.ServiceResolver.GetOrCreateGridItemPlacementService<TActorId>();
            var locationService = initParameter.ServiceResolver.GetOrCreateGridItemPlacementLocationService<TActorId>();

            foreach (var layer in mapLayers)
            {
                if (!map.TryGetGridDataFor(layer, out _))
                {
                    map.WithDefaultMapLayer(layer);
                }

                if (placementService.TryGetItemPlacementService(layer, out var existingPlacementService))
                {
                    if (placementService.TryGetItemPlacementLocationService(layer, out _))
                    {
                        continue;
                    }

                    placementService.WithLayer(layer, existingPlacementService, locationService);
                }
                else if (placementService.TryGetItemPlacementLocationService(layer, out var existingLocationService))
                {
                    placementService.WithLayer(layer, itemPlacementService, existingLocationService);
                }
                else
                {
                    placementService.WithLayer(layer, itemPlacementService, locationService);
                }
            }
        }

        void RegisterMapDataAggregate<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                            IGameLoopSystemRegistration context,
                                            EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            var sr = initParameter.ServiceResolver;
            if (sr.TryResolve(out AggregateMapStateController amc) && 
                sr.TryResolve(out IGridMapContext<TActorId> mapContext))
            {
                foreach (var ml in mapContext.GridLayers())
                {
                    if (mapContext.TryGetGridDataFor(ml, out var dataContext))
                    {
                        amc.Add(dataContext);
                    }
                }
            }
        }

        void RegisterClearGridPositionChangeTrackers<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                               IGameLoopSystemRegistration context,
                                                               EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            void ClearGridPositionAction()
            {
                registry.ResetComponent<EntityGridPositionChangedMarker>();
            }

            context.AddFixedStepHandlers(ClearGridPositionAction);
            EntityGridPositionChangedMarker.InstallChangeHandler(registry);
        }

        void RegisterClearDestroyedEntities<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                      IGameLoopSystemRegistration context,
                                                      EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            var sr = initParameter.ServiceResolver;
            var system = new GridPositionCleanUpSystem<TActorId, EntityGridPosition>(sr.Resolve<IItemPlacementServiceContext<TActorId>>());

            context.AddFixedStepHandlers(system.StartCollection);
            context.AddFixedStepHandlerSystem(registry.BuildSystem()
                                                      .WithoutContext()
                                                      .WithInputParameter<DestroyedMarker>()
                                                      .WithInputParameter<EntityGridPosition>()
                                                      .CreateSystem(system.CollectDestroyedEntities));
            context.AddFixedStepHandlers(system.RemoveCollectedEntitiesFromMap);

            context.AddInitializationStepHandler(system.StartCollection);
            context.AddInitializationStepHandlerSystem(registry.BuildSystem()
                                                               .WithoutContext()
                                                               .WithInputParameter<DestroyedMarker>()
                                                               .WithInputParameter<EntityGridPosition>()
                                                               .CreateSystem(system.CollectDestroyedEntities));
            context.AddInitializationStepHandler(system.RemoveCollectedEntitiesFromMap);
        }

        void RegisterGridEntities<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                            EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            registry.RegisterNonConstructable<EntityGridPosition>();
            registry.RegisterNonConstructable<EntityGridPositionChangedMarker>();
        }
    }
}
