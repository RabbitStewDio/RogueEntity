using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Common.ShadowCast;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Meta.EntityKeys;

namespace RogueEntity.Simple.Demo.BoxPusher
{
    [Module("BoxPusher")]
    public class BoxPusherModule : ModuleBase
    {

        public BoxPusherModule()
        {
            Id = "Game.BoxPusher";

            // DeclareDependencies(ModuleDependency.Of(InventoryModule.ModuleId),
            //                     ModuleDependency.Of(SensoryCacheModule.ModuleId),
            //                     ModuleDependency.Of(PositionModule.ModuleId),
            //                     ModuleDependency.Of(LightSourceModule.ModuleId),
            //                     ModuleDependency.Of(CoreModule.ModuleId));
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

            if (!mip.ServiceResolver.TryResolve(out IItemContextBackend<TGameContext, ActorReference> actorBackend))
            {
                actorBackend = new ItemContextBackend<TGameContext, ActorReference>(new ActorReferenceMetaData());
                mip.ServiceResolver.Store(actorBackend);
                mip.ServiceResolver.Store(actorBackend.ItemResolver);
                mip.ServiceResolver.Store(actorBackend.EntityMetaData);
            }

            if (!mip.ServiceResolver.TryResolve(out IItemResolver<TGameContext, ActorReference> _))
            {
                mip.ServiceResolver.Store(actorBackend.ItemResolver);
            }
            
            if (!mip.ServiceResolver.TryResolve(out IBulkDataStorageMetaData<ActorReference> _))
            {
                mip.ServiceResolver.Store(actorBackend.EntityMetaData);
            }

            if (!mip.ServiceResolver.TryResolve(out IItemContextBackend<TGameContext, ItemReference> itemBackend))
            {
                itemBackend = new ItemContextBackend<TGameContext, ItemReference>(new ItemReferenceMetaData());
                mip.ServiceResolver.Store(itemBackend);
                mip.ServiceResolver.Store(itemBackend.ItemResolver);
                mip.ServiceResolver.Store(itemBackend.EntityMetaData);
            }

            if (!mip.ServiceResolver.TryResolve(out IItemResolver<TGameContext, ItemReference> _))
            {
                mip.ServiceResolver.Store(itemBackend.ItemResolver);
            }

            if (!mip.ServiceResolver.TryResolve(out IBulkDataStorageMetaData<ItemReference> _))
            {
                mip.ServiceResolver.Store(actorBackend.EntityMetaData);
            }


            mip.ServiceResolver.GetOrCreateGridMapContext<ItemReference>();
            mip.ServiceResolver.GetOrCreateGridMapContext<ActorReference>();
        }

        [ContentInitializer]
        void InitializeContent<TGameContext>(in ModuleInitializationParameter mip, IModuleInitializer<TGameContext> initializer)

        {
            var serviceResolver = mip.ServiceResolver;
            var ctx = initializer.DeclareContentContext<ItemReference>();
            ctx.Activate(ctx.CreateBulkEntityBuilder(serviceResolver)
                            .DefineWall()
                            .Declaration);

            ctx.Activate(ctx.CreateBulkEntityBuilder(serviceResolver)
                            .DefineFloor()
                            .Declaration);

            ctx.Activate(ctx.CreateBulkEntityBuilder(serviceResolver)
                            .DefineFloorTargetZone()
                            .Declaration);

            ctx.Activate(ctx.CreateReferenceEntityBuilder(serviceResolver)
                            .DefineBox()
                            .Declaration);

            var actorContext = initializer.DeclareContentContext<ActorReference>();
            actorContext.Activate(actorContext.CreateReferenceEntityBuilder(serviceResolver)
                                              .DefinePlayer<TGameContext, ActorReference, ItemReference>()
                                              .Declaration);
        }
    }
}