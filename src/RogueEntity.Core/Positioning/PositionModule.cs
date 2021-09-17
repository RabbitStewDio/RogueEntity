using EnTTSharp.Entities;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.Meta;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Positioning.SpatialQueries;
using System.Linq;

namespace RogueEntity.Core.Positioning
{
    [Module]
    public class PositionModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.Position";

        public static readonly EntitySystemId RegisterCommonPositions = "Entities.Core.Position";
        public static readonly EntitySystemId RegisterGridPositions = "Entities.Core.Position.Grid";
        public static readonly EntitySystemId RegisterContinuousPositions = "Entities.Core.Position.Continuous";

        public static readonly EntitySystemId RegisterClearContinuousPositionChangeTracker = "Systems.Core.Position.Continuous.ClearChangeTracker";
        public static readonly EntitySystemId RegisterClearGridPositionChangeTracker = "Systems.Core.Position.Grid.ClearChangeTracker";
        public static readonly EntitySystemId RegisterSpatialQuery = "Systems.Core.Position.RegisterSpatialQuery";

        public static readonly EntityRole PositionQueryRole = new EntityRole("Role.Core.Position.PositionQueryable");

        public static readonly EntityRole PositionedRole = new EntityRole("Role.Core.Position.Positionable");
        public static readonly EntityRole GridPositionedRole = new EntityRole("Role.Core.Position.GridPositioned");
        public static readonly EntityRole ContinuousPositionedRole = new EntityRole("Role.Core.Position.ContinuousPositioned");

        public PositionModule()
        {
            Id = "Core.Position";
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Positioning";
            Description = "Provides support for positioning items in a grid or continuous coordinate system";
            IsFrameworkModule = true;

            RequireRole(ContinuousPositionedRole).WithImpliedRole(PositionedRole).WithDependencyOn(CoreModule.ModuleId);
            RequireRole(GridPositionedRole).WithImpliedRole(PositionedRole).WithDependencyOn(CoreModule.ModuleId);
        }

        [EntityRoleInitializer("Role.Core.Position.Positionable")]
        protected void InitializeCommon<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                  IModuleInitializer initializer,
                                                  EntityRole role)
            where TActorId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register(RegisterCommonPositions, 0, RegisterCommonEntities);

            if (!initParameter.ServiceResolver.TryResolve(out SpatialQueryRegistry r))
            {
                r = new SpatialQueryRegistry();
                initParameter.ServiceResolver.Store(r);
                initParameter.ServiceResolver.Store<ISpatialQueryLookup>(r);
            }
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

            if (!PositionModuleServices.TryGetOrCreateDefaultMapServices<TActorId>(initParameter.ServiceResolver, out var map, out var placementService))
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

        [EntityRoleFinalizerAttribute("Role.Core.Position.Positionable")]
        protected void FinalizePositionedRole<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                        IModuleInitializer initializer,
                                                        EntityRole role)
            where TActorId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TActorId>();
            ctx.Register(RegisterSpatialQuery, 9_999_999, RegisterSpatialQueryBruteForce);
        }

        void RegisterSpatialQueryBruteForce<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter, EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            if (!initParameter.ServiceResolver.TryResolve(out ISpatialQuery<TActorId> q))
            {
                q = new BruteForceSpatialQueryBackend<TActorId>(registry);
                initParameter.ServiceResolver.Store(q);

                if (initParameter.ServiceResolver.TryResolve(out SpatialQueryRegistry r))
                {
                    r.Register(q);
                }
            }
        }

        [EntityRoleInitializer("Role.Core.Position.GridPositioned")]
        protected void InitializeGridPositioned<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                          IModuleInitializer initializer,
                                                          EntityRole role)
            where TActorId : IEntityKey
        {
            EnsureDefaultGridMapExists(initParameter, initializer, role);
            
            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register(RegisterGridPositions, 0, RegisterGridEntities);
            entityContext.Register(RegisterClearGridPositionChangeTracker, 10, RegisterClearGridPositionChangeTrackers);
        }

        [EntityRoleInitializer("Role.Core.Position.ContinuousPositioned")]
        protected void InitializeContinuousPositioned<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                                IModuleInitializer initializer,
                                                                EntityRole role)
            where TActorId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register(RegisterContinuousPositions, 0, RegisterContinuousEntities);
            entityContext.Register(RegisterClearContinuousPositionChangeTracker, 10, RegisterClearContinuousPositionChangeTrackers);
        }

        void RegisterCommonEntities<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                              EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            registry.RegisterNonConstructable<ImmobilityMarker>();
        }

        void RegisterGridEntities<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                            EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            registry.RegisterNonConstructable<EntityGridPosition>();
            registry.RegisterNonConstructable<EntityGridPositionChangedMarker>();
        }

        void RegisterContinuousEntities<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                  EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            registry.RegisterNonConstructable<ContinuousMapPosition>();
            registry.RegisterNonConstructable<ContinuousMapPositionChangedMarker>();
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

        void RegisterClearContinuousPositionChangeTrackers<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                                     IGameLoopSystemRegistration context,
                                                                     EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
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
