using RogueEntity.Api.Modules;
using RogueEntity.Core.Runtime;
using System.Collections.Generic;

namespace RogueEntity.Core.Tests.Fixtures
{
    public class GameFixture<TPlayerEntity>: GameBase<TPlayerEntity>
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
