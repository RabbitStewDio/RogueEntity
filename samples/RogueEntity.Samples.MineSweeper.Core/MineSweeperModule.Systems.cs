using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Players;
using RogueEntity.Generator;
using RogueEntity.Samples.MineSweeper.Core.Commands;

namespace RogueEntity.Samples.MineSweeper.Core
{
    public partial class MineSweeperModule
    {
        [ModuleInitializer]
        void InitializeModule(in ModuleInitializationParameter mip, IModuleInitializer initializer)
        {
            mip.ServiceResolver.ConfigureEntity(ItemReferenceMetaData.Instance, MineSweeperMapLayers.Items, MineSweeperMapLayers.Flags);
            mip.ServiceResolver.ConfigureEntity(ActorReferenceMetaData.Instance);

            var mapBuilder = new MapBuilder().WithLayer<ItemReference>(MineSweeperMapLayers.Items, mip.ServiceResolver)
                                             .WithLayer<ItemReference>(MineSweeperMapLayers.Flags, mip.ServiceResolver);
            mip.ServiceResolver.Store(mapBuilder);
            mip.ServiceResolver.Store<IPlayerServiceConfiguration>(new PlayerServiceConfiguration(MineSweeperItemDefinitions.PlayerId));

            mip.ServiceResolver.Store(new MineSweeperCommandService<ActorReference>(mip.ServiceResolver.Resolve<IItemResolver<ActorReference>>()));
            mip.ServiceResolver.Store<IPlayerManager<ActorReference>>(
                new BasicPlayerManager<ActorReference>(
                    mip.ServiceResolver.Resolve<IItemResolver<ActorReference>>(),
                    mip.ServiceResolver.ResolveToReference<IPlayerServiceConfiguration>()));
        }
    }
}
