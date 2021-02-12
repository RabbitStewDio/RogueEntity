using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Players;

namespace RogueEntity.Simple.MineSweeper
{
    public partial class MineSweeperModule
    {
        [ContentInitializer]
        void InitializeContent(in ModuleInitializationParameter mip, IModuleInitializer initializer)
        {
            var serviceResolver = mip.ServiceResolver;
            var ctx = initializer.DeclareContentContext<ItemReference>();
            ctx.Activate(ctx.CreateBulkEntityBuilder(serviceResolver)
                            .DefineFloor()
                            .Declaration);
            
            ctx.Activate(ctx.CreateBulkEntityBuilder(serviceResolver)
                            .DefineWall()
                            .Declaration);

            ctx.Activate(ctx.CreateBulkEntityBuilder(serviceResolver)
                            .DefineFlag()
                            .Declaration);

            ctx.Activate(ctx.CreateReferenceEntityBuilder(serviceResolver)
                            .DefineMine()
                            .Declaration);

            var actorContext = initializer.DeclareContentContext<ActorReference>();
            var playerId = actorContext.Activate(actorContext.CreateReferenceEntityBuilder(serviceResolver)
                                                             .DefinePlayer()
                                                             .Declaration);

            mip.ServiceResolver.Store<IPlayerServiceConfiguration>(new PlayerServiceConfiguration(playerId));
        }
    }
}
