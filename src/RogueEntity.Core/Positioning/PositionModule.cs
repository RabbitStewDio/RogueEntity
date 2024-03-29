﻿using EnTTSharp.Entities;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.Positioning.SpatialQueries;

namespace RogueEntity.Core.Positioning
{
    [Module]
    public class PositionModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.Position";

        public static readonly EntitySystemId RegisterCommonPositionsEntitySystemId = "Entities.Core.Position";
        public static readonly EntitySystemId RegisterSpatialQuerySystemId = "Systems.Core.Position.RegisterSpatialQuery";
        public static readonly EntitySystemId RegisterResetMapDataSystemId = "Systems.Core.Position.RegisterResetMapData";
        public static readonly EntitySystemId ClearMapPositionChangeTrackerSystemId = "Systems.Core.Position.ClearChangeTracker";

        public static readonly EntityRole PositionQueryRole = new EntityRole("Role.Core.Position.PositionQueryable");
        public static readonly EntityRole PositionedRole = new EntityRole("Role.Core.Position.Positionable");

        public PositionModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Positioning";
            Description = "Provides support for positioning items in a grid or continuous coordinate system";
            IsFrameworkModule = true;
        }

        [EntityRoleInitializer("Role.Core.Position.Positionable")]
        protected void InitializeCommon<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                  IModuleInitializer initializer,
                                                  EntityRole role)
            where TActorId : struct, IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register(RegisterCommonPositionsEntitySystemId, 0, RegisterCommonEntities);
            entityContext.Register(ClearMapPositionChangeTrackerSystemId, 10, RegisterClearMapPositionChangeTrackers);
            entityContext.Register(RegisterResetMapDataSystemId, 100, RegisterResetMapData);

            if (!initParameter.ServiceResolver.TryResolve<SpatialQueryRegistry>(out var r))
            {
                r = new SpatialQueryRegistry();
                initParameter.ServiceResolver.Store(r);
                initParameter.ServiceResolver.Store<ISpatialQueryLookup>(r);
            }

            if (!initParameter.ServiceResolver.TryResolve<IMapStateController>(out _))
            {
                var mc = new AggregateMapStateController();
                initParameter.ServiceResolver.Store<IMapStateController>(mc);
                initParameter.ServiceResolver.Store(mc);
            }
        }

        void RegisterResetMapData<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter, 
                                            IGameLoopSystemRegistration context, 
                                            EntityRegistry<TActorId> registry)
            where TActorId : struct, IEntityKey
        {
            if (initParameter.ServiceResolver.TryResolve<IMapStateController>(out var mc))
            {
                context.AddInitializationStepHandler(mc.ResetState);
                context.AddDisposeStepHandler(mc.ResetState);
            }
        }

        void RegisterClearMapPositionChangeTrackers<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                              IGameLoopSystemRegistration context,
                                                              EntityRegistry<TActorId> registry)
            where TActorId : struct, IEntityKey
        {
            void ClearGridPositionAction()
            {
                registry.ResetComponent<MapPositionChangedMarker>();
            }

            context.AddFixedStepHandlers(ClearGridPositionAction);
        }

        void RegisterCommonEntities<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                              EntityRegistry<TActorId> registry)
            where TActorId : struct, IEntityKey
        {
            registry.RegisterNonConstructable<ImmobilityMarker>();
            registry.RegisterNonConstructable<MapPositionChangedMarker>();
        }

    }
}
