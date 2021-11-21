using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Runtime;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Tests.Fixtures
{
    public class GameFixture<TPlayerEntity>: SinglePlayerGameBase<TPlayerEntity>
    {
        readonly List<IModule> extraModules;

        public GameFixture(params string[] moduleIds) : base(moduleIds)
        {
            extraModules = new List<IModule>();
        }

        public void AddExtraModule(IModule module)
        {
            extraModules.Add(module);
        }

        public bool StartGame()
        {
            return base.StartGameWithPlayer();
        }

        public bool ReActivatePlayer(Guid playerId = default)
        {
            return base.ActivatePlayer(playerId);
        }

        protected override ModuleSystem CreateModuleSystem()
        {
            var ms = base.CreateModuleSystem();
            foreach (var m in extraModules)
            {
                ms.AddModule(m);
            }

            return ms;
        }
        

        public void AdvanceFrame(int frameTime = 1)
        {
            var tickIncrement = GameLoop.TimeSource.FixedTimeStep;
            var currentTime = GameLoop.TimeSource.CurrentTime;
            Update(currentTime + TimeSpan.FromTicks(tickIncrement.Ticks * frameTime));
        }

        public IItemResolver<ActorReference> ActorResolver => ServiceResolver.Resolve<IItemResolver<ActorReference>>();
        public IItemResolver<ItemReference> ItemResolver => ServiceResolver.Resolve<IItemResolver<ItemReference>>();
    }
}
