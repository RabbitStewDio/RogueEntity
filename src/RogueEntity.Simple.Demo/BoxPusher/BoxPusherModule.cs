using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Inventory;
using RogueEntity.Core.Meta;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Discovery;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Sensing.Sources.Light;

namespace RogueEntity.Simple.Demo.BoxPusher
{
    public class BoxPusherModule : ModuleBase
    {
        public static EntitySystemId SetUpGameSenseResistancesId = "System.Game.BoxPusher.SenseResistanceSystems";

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

            DeclareRelation<ActorReference, ItemReference>(InventoryModule.ContainsRelation);

            RequireRole(MovableItemRole);
        }


        /// <summary>
        ///   Changes to either floor or movable item maps should recompute the sense resistance data map.  
        /// </summary>
        [EntityRoleInitializer("Role.Core.Senses.Resistance.ResistanceDataProvider")]
        protected void InitializeRole<TGameContext, TItemId>(IModuleInitializer<TGameContext> initializer, EntityRole role)
            where TItemId : IEntityKey
            where TGameContext : IItemContext<TGameContext, TItemId>, IGridMapContext<TItemId>, IGridMapRawDataContext<TItemId>
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            if (role == MovableItemRole)
            {
                ctx.Register(SetUpGameSenseResistancesId, 1100, RegisterLayerConfiguration<TGameContext, TItemId>(BoxPusherMapLayers.Items));
            }
            else if (role == FloorRole)
            {
                ctx.Register(SetUpGameSenseResistancesId, 1100, RegisterLayerConfiguration<TGameContext, TItemId>(BoxPusherMapLayers.Floor));
            }
        }

        ModuleEntityContext.EntitySystemRegistrationDelegate<TGameContext, TItemId> RegisterLayerConfiguration<TGameContext, TItemId>(MapLayer layer)
            where TItemId : IEntityKey
            where TGameContext : IItemContext<TGameContext, TItemId>, IGridMapContext<TItemId>, IGridMapRawDataContext<TItemId>
        {
            void RegisterFloorItemResistanceSystemConfiguration(IServiceResolver serviceResolver,
                                                                IGameLoopSystemRegistration<TGameContext> context,
                                                                EntityRegistry<TItemId> registry,
                                                                ICommandHandlerRegistration<TGameContext, TItemId> handler)
            {
                var factory = serviceResolver.Resolve<ISensePropertiesSystem<TGameContext, VisionSense>>();
                factory.AddLayer<TGameContext, TItemId, VisionSense>(layer);

                var cache = serviceResolver.Resolve<ISenseCacheSetupSystem>();
                cache.RegisterCacheLayer(layer);
            }

            return RegisterFloorItemResistanceSystemConfiguration;
        }
    }
}