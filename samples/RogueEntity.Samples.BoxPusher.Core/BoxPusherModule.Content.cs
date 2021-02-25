﻿using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Players;

namespace RogueEntity.Samples.BoxPusher.Core
{
    public partial class BoxPusherModule
    {
        
        [ContentInitializer]
        void InitializeContent(in ModuleInitializationParameter mip, IModuleInitializer initializer)
        {
            var serviceResolver = mip.ServiceResolver;
            var ctx = initializer.DeclareContentContext<ItemReference>();
            ctx.Activate(ctx.CreateBulkEntityBuilder(serviceResolver)
                            .DefineWall()
                            .Declaration);

            ctx.Activate(ctx.CreateBulkEntityBuilder(serviceResolver)
                            .DefineFloor()
                            .Declaration);

            ctx.Activate(ctx.CreateReferenceEntityBuilder(serviceResolver)
                            .DefineFloorTargetZone()
                            .Declaration);

            ctx.Activate(ctx.CreateReferenceEntityBuilder(serviceResolver)
                            .DefineBox()
                            .Declaration);

            ctx.Activate(ctx.CreateReferenceEntityBuilder(serviceResolver)
                            .DefineSpawnPoint()
                            .Declaration);

            var actorContext = initializer.DeclareContentContext<ActorReference>();
            var playerId = actorContext.Activate(actorContext.CreateReferenceEntityBuilder(serviceResolver)
                                                             .DefinePlayer<ActorReference, ItemReference>()
                                                             .Declaration);

            mip.ServiceResolver.Store<IPlayerServiceConfiguration>(new PlayerServiceConfiguration(playerId));
        }

    }
}
