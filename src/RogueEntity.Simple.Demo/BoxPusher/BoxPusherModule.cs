using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Inventory;
using RogueEntity.Core.Meta;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.Naming;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Common.ShadowCast;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Simple.Demo.BoxPusher
{
    [Module]
    public class BoxPusherModule : ModuleBase
    {
        public static EntityRole MovableItemRole = new EntityRole("Role.Game.BoxPusher.MovableItem");
        public static EntityRole FloorRole = new EntityRole("Role.Game.BoxPusher.Floor");
        public static EntityRole ActorRole = new EntityRole("Role.Game.BoxPusher.Actors");

        public BoxPusherModule()
        {
            Id = "Game.BoxPusher";

            DeclareDependencies(ModuleDependency.Of(InventoryModule.ModuleId),
                                ModuleDependency.Of(SensoryCacheModule.ModuleId),
                                ModuleDependency.Of(PositionModule.ModuleId),
                                ModuleDependency.Of(LightSourceModule.ModuleId),
                                ModuleDependency.Of(CoreModule.ModuleId));

            DeclareEntity<ItemReference>(MovableItemRole);
            // .WithImpliedRole(CoreModule.ItemRole)
            // .WithImpliedRole(PositionModule.GridPositionedRole)
            // .WithImpliedRole(InventoryModule.ContainedItemRole)
            // .WithImpliedRole(LightSourceModule.SenseSourceRole)
            // .WithImpliedRole(LightSourceModule.ResistanceDataProviderRole);

            DeclareEntity<ItemReference>(FloorRole);
            // .WithImpliedRole(CoreModule.ItemRole)
            // .WithImpliedRole(PositionModule.GridPositionedRole)
            // .WithImpliedRole(LightSourceModule.ResistanceDataProviderRole);

            DeclareEntity<ActorReference>(ActorRole);
            // .WithImpliedRole(CoreModule.ItemRole)
            // .WithImpliedRole(CoreModule.PlayerRole)
            // .WithImpliedRole(PositionModule.GridPositionedRole)
            // .WithImpliedRole(InventoryModule.ContainerRole)
            // .WithImpliedRole(VisionSenseModule.SenseReceptorActorRole)
            // .WithImpliedRole(SensoryCacheModule.SenseCacheSourceRole)
            // .WithImpliedRole(SenseDiscoveryModule.DiscoveryActorRole);

            RequireRole(MovableItemRole);
        }


        [ModuleInitializer]
        void InitializeModule<TGameContext>(in ModuleInitializationParameter mip, IModuleInitializer<TGameContext> initializer)
        {
            if (!mip.ServiceResolver.TryResolve(out ILightPhysicsConfiguration lightPhysics))
            {
                if (!mip.ServiceResolver.TryResolve(out ShadowPropagationResistanceDataSource ds))
                {
                    ds = new ShadowPropagationResistanceDataSource();
                    mip.ServiceResolver.Store(ds);
                }

                lightPhysics = new LightPhysicsConfiguration(LinearDecaySensePhysics.For(DistanceCalculation.Euclid), ds);
                mip.ServiceResolver.Store(lightPhysics);
            }

            if (!mip.ServiceResolver.TryResolve(out IItemContextBackend<TGameContext, ActorReference> a))
            {
                a = new ItemContextBackend<TGameContext, ActorReference>(new ActorReferenceMetaData());
                mip.ServiceResolver.Store(a);
                mip.ServiceResolver.Store(a.ItemResolver);
            }

            if (!mip.ServiceResolver.TryResolve(out IItemContextBackend<TGameContext, ItemReference> i))
            {
                i = new ItemContextBackend<TGameContext, ItemReference>(new ItemReferenceMetaData());
                mip.ServiceResolver.Store(i);
                mip.ServiceResolver.Store(i.ItemResolver);
            }
        }

        [ContentInitializer]
        void InitializeContent<TGameContext>(in ModuleInitializationParameter mip, IModuleInitializer<TGameContext> initializer)

        {
            var serviceResolver = mip.ServiceResolver;
            var ctx = initializer.DeclareContentContext<ItemReference>();
            ctx.Activate(ctx.CreateBulkEntityBuilder(serviceResolver)
                            .Define("Items.Wall")
                            .WithGridPosition(MapLayer.Indeterminate)
                            .AsImmobile()
                            .WithLightResistance(100.Percent())
                            .WithName("wall")
                            .Declaration);

            ctx.CreateBulkEntityBuilder(serviceResolver)
               .RedefineAs("Items.Wall", "Item.Wall.Stone");

            var actorContext = initializer.DeclareContentContext<ActorReference>();
            actorContext.Activate(actorContext.CreateReferenceEntityBuilder(serviceResolver)
                                              .Define("Player")
                                              .AsPlayer()
                                              .WithInventory()
                                              .Of<ItemReference>()
                                              .WithLightSource(10)
                                              .WithVisionSense(10)
                                              .Declaration);
        }
/*
        /// <summary>
        ///   Changes to either floor or movable item maps should recompute the sense resistance data map.  
        /// </summary>
        [EntityRoleInitializer("Role.Game.BoxPusher.MovableItem",
                               ConditionalRoles = new[] {"Role.Core.Senses.Resistance.ResistanceDataProvider"})]
        protected void InitializeItemRole<TGameContext, TItemId>(in ModuleInitializationParameter p, IModuleInitializer<TGameContext> initializer, EntityRole role)
            where TItemId : IEntityKey
            where TGameContext : IItemContext<TGameContext, TItemId>, IGridMapContext<TItemId>
        {
            var systemId = SenseSourceModules.CreateResistanceSourceSystemId<VisionSense>(nameof(BoxPusherMapLayers.Items));
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(systemId, 1100, SenseSourceModules.RegisterSenseResistanceSourceLayer<TGameContext, TItemId, VisionSense>(BoxPusherMapLayers.Items));
        }

        /// <summary>
        ///   Changes to either floor or movable item maps should recompute the sense resistance data map.  
        /// </summary>
        [EntityRoleInitializer("Role.Game.BoxPusher.Floor",
                               ConditionalRoles = new[] {"Role.Core.Senses.Resistance.ResistanceDataProvider"})]
        protected void InitializeFloorRole<TGameContext, TItemId>(in ModuleInitializationParameter p, IModuleInitializer<TGameContext> initializer, EntityRole role)
            where TItemId : IEntityKey
            where TGameContext : IItemContext<TGameContext, TItemId>, IGridMapContext<TItemId>
        {
            var systemId = SenseSourceModules.CreateResistanceSourceSystemId<VisionSense>(nameof(BoxPusherMapLayers.Floor));
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(systemId, 1100, SenseSourceModules.RegisterSenseResistanceSourceLayer<TGameContext, TItemId, VisionSense>(BoxPusherMapLayers.Floor));
        }
        */
    }
}