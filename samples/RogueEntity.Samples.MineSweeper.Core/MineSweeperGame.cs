using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Services;
using RogueEntity.Api.Time;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Runtime;
using RogueEntity.Samples.MineSweeper.Core.Commands;
using RogueEntity.Samples.MineSweeper.Core.Services;
using RogueEntity.Samples.MineSweeper.Core.Traits;
using System;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Samples.MineSweeper.Core
{
    public class MineSweeperGame: GameBase<ActorReference>
    {
        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        readonly DirectoryCatalog pluginCatalogue;

        DefaultRandomGeneratorSource randomGeneratorSource;

        public MineSweeperGameParameterService GameParameterService { get; }
        public BasicCommandService<ActorReference> CommandService { get; private set; }

        public MineSweeperGame(): base("MineSweeper")
        {
            pluginCatalogue = new DirectoryCatalog(".");
            GameParameterService = new MineSweeperGameParameterService();
            GameInitialized += OnGameInitialized;
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            CommandService = ServiceResolver.Resolve<BasicCommandService<ActorReference>>();
        }

        protected override void InitializeServices(IServiceResolver serviceResolver)
        {
            base.InitializeServices(serviceResolver);
            randomGeneratorSource = new DefaultRandomGeneratorSource(10, serviceResolver.ResolveToReference<ITimeSource>());
            serviceResolver.Store<IEntityRandomGeneratorSource>(randomGeneratorSource);
            serviceResolver.Store<IMineSweeperGameParameterService>(GameParameterService);
        }

        /// <summary>
        ///    Starts a new game, either using the existing player data identified by the given Guid
        ///    or a new, randomly generated player id.
        /// </summary>
        public bool StartGame(MineSweeperGameParameter param)
        {
            if (Status == GameStatus.Running)
            {
                return false;
            }

            GameParameterService.WorldParameter = param;
            randomGeneratorSource.Seed = param.Seed;

            return PerformStartGame();
        }

        protected override GameStatus CheckStatus()
        {
            if (!PlayerData.TryGetValue(out var pd))
            {
                return GameStatus.Initialized;
            }

            var actorResolver = ServiceResolver.Resolve<IItemResolver<ActorReference>>();
            if (actorResolver.TryQueryData(pd.EntityId, out MineSweeperPlayerData data))
            {
                if (data.ExplodedPosition.HasValue)
                {
                    return GameStatus.GameLost;
                }

                if (data.AreaCleared)
                {
                    return GameStatus.GameWon;
                }
            }

            return GameStatus.Running;
        }

    }
}
