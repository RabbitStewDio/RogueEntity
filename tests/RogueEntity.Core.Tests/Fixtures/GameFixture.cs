using RogueEntity.Api.Modules;
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
    }
}
