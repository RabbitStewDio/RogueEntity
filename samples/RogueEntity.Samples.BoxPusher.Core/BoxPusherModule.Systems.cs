using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.MapLoading.Builder;
using RogueEntity.Core.MapLoading.FlatLevelMaps;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.MapLoading.PlayerSpawning;
using RogueEntity.Core.Players;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Core.Storage;
using RogueEntity.Generator;
using RogueEntity.Samples.BoxPusher.Core.ItemTraits;

namespace RogueEntity.Samples.BoxPusher.Core
{
    public partial class BoxPusherModule
    {
        [ModuleInitializer]
        void InitializeModule(in ModuleInitializationParameter mip, IModuleInitializer initializer)
        {
            mip.ServiceResolver.ConfigureLightPhysics();
        }

        [LateModuleInitializer]
        void InitializeLateModule(in ModuleInitializationParameter mip, IModuleInitializer initializer)
        {
            mip.ServiceResolver.Store(new BoxPusherWinConditionSystems());

            var mapLoader = new DirectoryMapLevelDataSource(mip.ServiceResolver.ResolveToReference<MapBuilder>(),
                                                            mip.ServiceResolver.Resolve<IStorageLocationService>(),
                                                            mip.ServiceResolver.Resolve<IEntityRandomGeneratorSource>());
            mip.ServiceResolver.Store<IMapRegionLoadingStrategy<int>>(mapLoader);
            mip.ServiceResolver.Store<IMapRegionMetaDataService<int>>(mapLoader);
            
            var evictionHandler = new FlatLevelRegionEvictionStrategy(mip.ServiceResolver.ResolveToReference<MapBuilder>(), mapLoader);
            mip.ServiceResolver.Store<IMapRegionEvictionStrategy<int>>(evictionHandler);

            var profileManager = CreateProfileManager();
            mip.ServiceResolver.Store(profileManager);
            mip.ServiceResolver.Store<IFlatLevelPlayerSpawnInformationSource>(new BoxPusherSpawnInfoSource(profileManager));
        }

        IPlayerProfileManager<BoxPusherPlayerProfile> CreateProfileManager()
        {
            var profileManager = new InMemoryPlayerProfileManager<BoxPusherPlayerProfile>();
            profileManager.TryCreatePlayer(new BoxPusherPlayerProfile("Duffy Duck"), out _, out _);
            profileManager.TryCreatePlayer(new BoxPusherPlayerProfile("Bugs Bunny").RecordLevelComplete(1), out _, out _);
            return profileManager;
        }
    }
}
