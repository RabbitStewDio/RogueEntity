using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core;
using RogueEntity.Core.Chunks;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Players;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Generator;
using RogueEntity.Generator.MapFragments;
using RogueEntity.Simple.BoxPusher.ItemTraits;

namespace RogueEntity.Simple.BoxPusher
{
    public partial class BoxPusherModule
    {
        [ModuleInitializer]
        void InitializeModule(in ModuleInitializationParameter mip, IModuleInitializer initializer)
        {
            mip.ServiceResolver.ConfigureLightPhysics();
            mip.ServiceResolver.ConfigureEntity(ItemReferenceMetaData.Instance, BoxPusherMapLayers.Floor, BoxPusherMapLayers.Items);
            mip.ServiceResolver.ConfigureEntity(ActorReferenceMetaData.Instance, BoxPusherMapLayers.Actors);

            var mapBuilder = new MapBuilder().WithLayer<ItemReference>(BoxPusherMapLayers.Floor, mip.ServiceResolver)
                                             .WithLayer<ItemReference>(BoxPusherMapLayers.Items, mip.ServiceResolver)
                                             .WithLayer<ActorReference>(BoxPusherMapLayers.Actors, mip.ServiceResolver);
            var fragmentParser = new MapFragmentParser();
            var mapLoader = new BoxPusherMapLevelDataSource(mapBuilder, fragmentParser, mip.ServiceResolver.Resolve<IEntityRandomGeneratorSource>());
            mip.ServiceResolver.Store<IMapLevelDataSource<int>>(mapLoader);
            mip.ServiceResolver.Store<IMapLevelDataSourceSystem>(mapLoader);

            /*
            var profileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RogueEntity/BoxPusher");
            var profileDataRepoFact = new FileDataRepositoryFactory(profileDir, MessagePackSerializerOptions.Standard)
                                      .WithKey(new GuidValueConverter())
                                      .WithKey(new StringValueConverter());
            
            var profileManager = new DefaultPlayerProfileManager<BoxPusherPlayerProfile>(profileDataRepoFact.Create<Guid, BoxPusherPlayerProfile>("profiles"));
            */
            var profileManager = new InMemoryPlayerProfileManager<BoxPusherPlayerProfile>();
            profileManager.TryCreatePlayer(new BoxPusherPlayerProfile("Duffy Duck"), out _, out _);
            profileManager.TryCreatePlayer(new BoxPusherPlayerProfile("Bugs Bunny").RecordLevelComplete(1), out _, out _);
            mip.ServiceResolver.Store<IPlayerProfileManager<BoxPusherPlayerProfile>>(profileManager);

            mip.ServiceResolver.Store(new BoxPusherWinConditionSystems());
            mip.ServiceResolver.Store(BoxPusherLevelSystem<ActorReference, ItemReference>.Create(mip.ServiceResolver, BoxPusherMapLayers.Actors));
            mip.ServiceResolver.Store<IPlayerManager<ActorReference, BoxPusherPlayerProfile>>(
                new InMemoryPlayerManager<ActorReference, BoxPusherPlayerProfile>(
                    mip.ServiceResolver.Resolve<IItemResolver<ActorReference>>(),
                    mip.ServiceResolver.ResolveToReference<IPlayerServiceConfiguration>(),
                    profileManager));
        }
    }
}
