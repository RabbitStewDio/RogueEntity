using RogueEntity.Api.Services;
using RogueEntity.Api.Time;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Players;
using RogueEntity.Core.Runtime;
using RogueEntity.Samples.BoxPusher.Core.ItemTraits;
using System;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Samples.BoxPusher.MonoGame
{
    public class BoxPusherGame: GameBase<ActorReference>
    {
        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        readonly DirectoryCatalog pluginCatalogue;

        public IPlayerProfileManager<BoxPusherPlayerProfile> ProfileManager { get; private set; }

        public BoxPusherGame(): base("BoxPusher")
        {
            pluginCatalogue = new DirectoryCatalog(".");
            GameInitialized += OnGameInitialized;
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            ProfileManager = ServiceResolver.Resolve<IPlayerProfileManager<BoxPusherPlayerProfile>>();
        }

        protected override GameStatus CheckStatus()
        {
            return GameStatus.Running;
        }

        protected override void InitializeServices(IServiceResolver serviceResolver)
        {
            serviceResolver.Store<IEntityRandomGeneratorSource>(new DefaultRandomGeneratorSource(10, ServiceResolver.ResolveToReference<ITimeSource>()));
        }

        public bool StartGame(Guid playerId = default) => PerformStartGame(playerId);
    }
}