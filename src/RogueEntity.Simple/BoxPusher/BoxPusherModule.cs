using EnTTSharp.Entities;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Services;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.Movement.MovementModes.Walking;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using RogueEntity.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using static RogueEntity.Core.Movement.CostModifier.MovementCostModifiers;

namespace RogueEntity.Simple.BoxPusher
{
    [Module("BoxPusher")]
    public class BoxPusherModule : ModuleBase
    {
        public BoxPusherModule()
        {
            Id = "Game.BoxPusher";
        }

        [ModuleInitializer]
        void InitializeModule(in ModuleInitializationParameter mip, IModuleInitializer initializer)
        {
            mip.ServiceResolver.ConfigureLightPhysics();

            mip.ServiceResolver.ConfigureEntityType(ItemReferenceMetaData.Instance);
            mip.ServiceResolver.GetOrCreateDefaultGridMapContext<ItemReference>()
               .WithDefaultMapLayer(BoxPusherMapLayers.Floor)
               .WithDefaultMapLayer(BoxPusherMapLayers.Items);

            var itemPlacementContext = new ItemPlacementServiceContext<ItemReference>()
                                       .WithLayer(BoxPusherMapLayers.Floor,
                                                  mip.ServiceResolver.GetOrCreateGridItemPlacementService<ItemReference>(),
                                                  mip.ServiceResolver.GetOrCreateGridItemPlacementLocationService<ItemReference>())
                                       .WithLayer(BoxPusherMapLayers.Items, 
                                                  mip.ServiceResolver.GetOrCreateGridItemPlacementService<ItemReference>(),
                                                  mip.ServiceResolver.GetOrCreateGridItemPlacementLocationService<ItemReference>());
            mip.ServiceResolver.Store<IItemPlacementServiceContext<ItemReference>>(itemPlacementContext);


            mip.ServiceResolver.ConfigureEntityType(ActorReferenceMetaData.Instance);
            mip.ServiceResolver.GetOrCreateDefaultGridMapContext<ActorReference>()
               .WithDefaultMapLayer(BoxPusherMapLayers.Actors);

            var actorPlacementContext = new ItemPlacementServiceContext<ActorReference>()
                .WithLayer(BoxPusherMapLayers.Actors, 
                           mip.ServiceResolver.GetOrCreateGridItemPlacementService<ActorReference>(),
                           mip.ServiceResolver.GetOrCreateGridItemPlacementLocationService<ActorReference>());
            mip.ServiceResolver.Store<IItemPlacementServiceContext<ActorReference>>(actorPlacementContext);
        }

        [ContentInitializer]
        void InitializeContent(in ModuleInitializationParameter mip, IModuleInitializer initializer)
        {
            var serviceResolver = mip.ServiceResolver;
            var ctx = initializer.DeclareContentContext<ItemReference>();
            ctx.Activate(ctx.CreateBulkEntityBuilder(serviceResolver)
                            .DefineWall()
                            .WithMovementCostModifier(Blocked<WalkingMovement>())
                            .Declaration);

            ctx.Activate(ctx.CreateBulkEntityBuilder(serviceResolver)
                            .DefineFloor()
                            .WithMovementCostModifier(For<WalkingMovement>(1))
                            .Declaration);

            ctx.Activate(ctx.CreateBulkEntityBuilder(serviceResolver)
                            .DefineFloorTargetZone()
                            .WithMovementCostModifier(For<WalkingMovement>(1))
                            .Declaration);

            ctx.Activate(ctx.CreateReferenceEntityBuilder(serviceResolver)
                            .DefineBox()
                            .WithMovementCostModifier(Blocked<WalkingMovement>())
                            .Declaration);

            var actorContext = initializer.DeclareContentContext<ActorReference>();
            var playerId = actorContext.Activate(actorContext.CreateReferenceEntityBuilder(serviceResolver)
                                                             .DefinePlayer<ActorReference, ItemReference>()
                                                             .WithMovement()
                                                             .AsPointCost(WalkingMovement.Instance, DistanceCalculation.Euclid, 1)
                                                             .Declaration);

            mip.ServiceResolver.Store<IPlayerServiceConfiguration>(new PlayerServiceConfiguration(playerId));
        }
    }

    public class BoxPusherSystems
    {
        readonly IGridMapContext<ItemReference> map;
        readonly IGridMapDataContext<ItemReference> itemLayer;

        EntityGridPosition playerPosition;
        HashSet<Position2D> targetSpots;
        HashSet<Position2D> boxPositions;
        IReadOnlyDynamicDataView2D<ItemReference> currentLevelMap;

        public BoxPusherSystems(IGridMapContext<ItemReference> map)
        {
            this.map = map;
            if (!map.TryGetGridDataFor(BoxPusherMapLayers.Items, out itemLayer))
            {
                throw new ArgumentException("Require 'ItemLayer@ in map");
            }
        }

        public void StartCheckWinCondition()
        {
            targetSpots.Clear();
            boxPositions.Clear();
            currentLevelMap = null;
            playerPosition = default;
        }

        public void FindPlayer(IEntityViewControl<ActorReference> actors, ActorReference k, in EntityGridPosition pos, in PlayerTag playerTag)
        {
            itemLayer.TryGetView(pos.GridZ, out currentLevelMap);
            playerPosition = pos;
        }
        
        public void CollectTargetSpots(IEntityViewControl<ItemReference> items, ItemReference k, in EntityGridPosition pos, in BoxPusherTargetFieldMarker targetMarker)
        {
            if (currentLevelMap == null)
            {
                return;
            }

            if (pos.IsInvalid || pos.GridZ != playerPosition.GridZ)
            {
                return;
            }

            targetSpots.Add(pos.ToGridXY());
        }
        
        public void CollectBoxPositions(IEntityViewControl<ItemReference> items, ItemReference k, in EntityGridPosition pos, in BoxPusherBoxMarker targetMarker)
        {
            if (currentLevelMap == null)
            {
                return;
            }

            if (pos.IsInvalid || pos.GridZ != playerPosition.GridZ)
            {
                return;
            }

            boxPositions.Add(pos.ToGridXY());
        }

        public void FinishEvaluateWinCondition(IEntityViewControl<ActorReference> actors, ActorReference k, in EntityGridPosition pos, in PlayerTag playerTag)
        {
            if (boxPositions.SetEquals(targetSpots))
            {
                
            }
            
        }
    }

    [MessagePackObject]
    [DataContract]
    public class BoxPusherLevelStats
    {
        [Key(0)]
        [DataMember(Order=0)]
        readonly Dictionary<int, LevelStats> levelStats;

        public BoxPusherLevelStats()
        {
            this.levelStats = new Dictionary<int, LevelStats>();
        }

        internal BoxPusherLevelStats(Dictionary<int, LevelStats> levelStats)
        {
            this.levelStats = levelStats;
        }
    }

    [MessagePackObject]
    [DataContract]
    public readonly struct LevelStats
    {
        [Key(0)]
        [DataMember(Order=0)]
        public readonly int Steps;
        [Key(1)]
        [DataMember(Order=1)]
        public readonly bool ClearedOnce;
        [Key(2)]
        [DataMember(Order=2)]
        public readonly bool ClearedNow;

        public LevelStats(int steps, bool clearedOnce, bool clearedNow)
        {
            Steps = steps;
            ClearedOnce = clearedOnce;
            ClearedNow = clearedNow;
        }
    }
    
    [EntityComponent(EntityConstructor.Flag)]
    public readonly struct BoxPusherTargetFieldMarker
    {
    }
    
    [EntityComponent(EntityConstructor.Flag)]
    public readonly struct BoxPusherBoxMarker
    {
    }
}
