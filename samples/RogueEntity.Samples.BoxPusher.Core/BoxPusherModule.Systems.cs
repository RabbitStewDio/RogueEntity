using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Time;
using RogueEntity.Core;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Core.MapLoading;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.GridMovement;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Core.Storage;
using RogueEntity.Generator;
using RogueEntity.Generator.MapFragments;
using RogueEntity.Samples.BoxPusher.Core.Commands;
using RogueEntity.Samples.BoxPusher.Core.ItemTraits;
using System;

namespace RogueEntity.Samples.BoxPusher.Core
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
            var mapLoader = new BoxPusherMapLevelDataSource(mapBuilder, fragmentParser,
                                                            mip.ServiceResolver.Resolve<IStorageLocationService>(),
                                                            mip.ServiceResolver.Resolve<IEntityRandomGeneratorSource>());
            mip.ServiceResolver.Store<IMapRegionLoaderService<int>>(mapLoader);
            mip.ServiceResolver.Store<IBoxPusherMapMetaDataService>(mapLoader);

            mip.ServiceResolver.Store(new BasicCommandService<ActorReference>(mip.ServiceResolver.Resolve<IItemResolver<ActorReference>>()));

            var profileManager = new InMemoryPlayerProfileManager<BoxPusherPlayerProfile>();
            profileManager.TryCreatePlayer(new BoxPusherPlayerProfile("Duffy Duck"), out _, out _);
            profileManager.TryCreatePlayer(new BoxPusherPlayerProfile("Bugs Bunny").RecordLevelComplete(1), out _, out _);
            mip.ServiceResolver.Store<IPlayerProfileManager<BoxPusherPlayerProfile>>(profileManager);

            mip.ServiceResolver.Store(new BoxPusherWinConditionSystems());
            mip.ServiceResolver.Store<IMapRegionSystem>(new BoxPusherMapRegionSystem(mapLoader, TimeSpan.FromMilliseconds(5), profileManager));
            mip.ServiceResolver.Store<IPlayerManager<ActorReference>>(
                new BasicPlayerManager<ActorReference>(
                    mip.ServiceResolver.Resolve<IItemResolver<ActorReference>>(),
                    mip.ServiceResolver.ResolveToReference<IPlayerServiceConfiguration>()));

        }
    }
}
