using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;

namespace RogueEntity.Samples.MineSweeper.Core
{
    public partial class MineSweeperModule
    {
        [LateModuleInitializer]
        void InitializeModule(in ModuleInitializationParameter mip, IModuleInitializer initializer)
        {
        }
    }
}
