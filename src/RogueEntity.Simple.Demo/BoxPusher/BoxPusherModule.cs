using EnTTSharp.Entities;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Inventory;
using RogueEntity.Core.Meta;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.Naming;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Discovery;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Core.Utils;

namespace RogueEntity.Simple.Demo.BoxPusher
{
    [Module]
    public class BoxPusherModule : ModuleBase
    {
        public static EntitySystemId SetUpItemSenseResistancesId = "System.Game.BoxPusher.SenseResistanceSystems.Items";
        public static EntitySystemId SetUpFloorSenseResistancesId = "System.Game.BoxPusher.SenseResistanceSystems.Floor";

        public static EntityRole MovableItemRole = new EntityRole("Role.Game.BoxPusher.MovableItem");
        public static EntityRole FloorRole = new EntityRole("Role.Game.BoxPusher.Floor");
        public static EntityRole ActorRole = new EntityRole("Role.Game.BoxPusher.Actors");

        public BoxPusherModule()
        {
            Id = "Game.BoxPusher";

            DeclareDependencies(ModuleDependency.Of(InventoryModule.ModuleId),
                                ModuleDependency.Of(SensoryCacheModule.ModuleId),
                                ModuleDependency.Of(PositionModule.ModuleId),
                                ModuleDependency.Of(CoreModule.ModuleId));

            DeclareEntity<ItemReference>(MovableItemRole)
                .WithImpliedRole(CoreModule.ItemRole)
                .WithImpliedRole(PositionModule.GridPositionedRole)
                .WithImpliedRole(InventoryModule.ContainedItemRole)
                .WithImpliedRole(LightSourceModule.SenseSourceRole)
                .WithImpliedRole(LightSourceModule.ResistanceDataProviderRole);

            DeclareEntity<ItemReference>(FloorRole)
                .WithImpliedRole(CoreModule.ItemRole)
                .WithImpliedRole(PositionModule.GridPositionedRole)
                .WithImpliedRole(LightSourceModule.ResistanceDataProviderRole);

            DeclareEntity<ActorReference>(ActorRole)
                .WithImpliedRole(CoreModule.ItemRole)
                .WithImpliedRole(CoreModule.PlayerRole)
                .WithImpliedRole(PositionModule.GridPositionedRole)
                .WithImpliedRole(InventoryModule.ContainerRole)
                .WithImpliedRole(VisionSenseModule.SenseReceptorActorRole)
                .WithImpliedRole(SensoryCacheModule.SenseCacheSourceRole)
                .WithImpliedRole(SenseDiscoveryModule.DiscoveryActorRole);

            RequireRole(MovableItemRole);
        }

        [ContentInitializer]
        void InitializeContent<TGameContext>(IServiceResolver serviceResolver, IModuleInitializer<TGameContext> initializer)

        {
            var ctx = initializer.DeclareEntityContext<ItemReference>();
            ctx.CreateBulkEntityBuilder(serviceResolver)
               .Define("Items.Wall")
               .WithGridPosition(MapLayer.Indeterminate)
               .AsImmobile()
               .WithLightResistance(100.Percent())
               .WithName("wall");

            ctx.CreateBulkEntityBuilder(serviceResolver)
               .RedefineAs("Items.Wall", "Item.Wall.Stone");
        }

        /// <summary>
        ///   Changes to either floor or movable item maps should recompute the sense resistance data map.  
        /// </summary>
        [EntityRoleInitializer("Role.Game.BoxPusher.MovableItem",
                               ConditionalRoles = new[] {"Role.Core.Senses.Resistance.ResistanceDataProvider"})]
        protected void InitializeItemRole<TGameContext, TItemId>(IServiceResolver serviceResolver, IModuleInitializer<TGameContext> initializer, EntityRole role)
            where TItemId : IEntityKey
            where TGameContext : IItemContext<TGameContext, TItemId>, IGridMapContext<TItemId>
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(SetUpItemSenseResistancesId, 1100, RegisterLayerConfiguration<TGameContext, TItemId>(BoxPusherMapLayers.Items));
        }

        /// <summary>
        ///   Changes to either floor or movable item maps should recompute the sense resistance data map.  
        /// </summary>
        [EntityRoleInitializer("Role.Game.BoxPusher.Floor",
                               ConditionalRoles = new[] {"Role.Core.Senses.Resistance.ResistanceDataProvider"})]
        protected void InitializeFloorRole<TGameContext, TItemId>(IServiceResolver serviceResolver, IModuleInitializer<TGameContext> initializer, EntityRole role)
            where TItemId : IEntityKey
            where TGameContext : IItemContext<TGameContext, TItemId>, IGridMapContext<TItemId>
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(SetUpFloorSenseResistancesId, 1100, RegisterLayerConfiguration<TGameContext, TItemId>(BoxPusherMapLayers.Floor));
        }

        EntitySystemRegistrationDelegate<TGameContext, TItemId> RegisterLayerConfiguration<TGameContext, TItemId>(MapLayer layer)
            where TItemId : IEntityKey
            where TGameContext : IItemContext<TGameContext, TItemId>, IGridMapContext<TItemId>
        {
            void RegisterItemResistanceSystemConfiguration(in ModuleInitializationParameter initParameter,
                                                           IGameLoopSystemRegistration<TGameContext> context,
                                                           EntityRegistry<TItemId> registry,
                                                           ICommandHandlerRegistration<TGameContext, TItemId> handler)
            {
                var serviceResolver = initParameter.ServiceResolver;
                var itemContext = serviceResolver.Resolve<IItemContext<TGameContext, TItemId>>();
                var mapContext = serviceResolver.Resolve<IGridMapContext<TItemId>>();
                var factory = serviceResolver.Resolve<IAggregationLayerSystem<TGameContext, SensoryResistance<VisionSense>>>();
                factory.AddLayer(mapContext, itemContext, layer);

                var cache = serviceResolver.Resolve<ISenseCacheSetupSystem>();
                cache.RegisterCacheLayer(layer);
            }

            return RegisterItemResistanceSystemConfiguration;
        }
    }
}