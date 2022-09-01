using RogueEntity.Api.GameLoops;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.MapLoading.FlatLevelMaps;
using RogueEntity.Core.MapLoading.MapRegions;

namespace RogueEntity.Generator
{
    public class GeneratorModule : ModuleBase
    {
        public static readonly string ModuleId = "Extras.Generator";

        static readonly EntitySystemId RegisterInitializeMapLoaderSystemId = "Systems.Extras.Generator.RegisterMapLoader";

        // Role.Core.Inputs.Commands.Executor[RogueEntity.Generator.Commands.ChangeLevelCommand]

        public GeneratorModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Extras.Generator";
            Name = "RogueEntity Map Generator Module";
            Description = "Provides services to generate and load maps";
            IsFrameworkModule = true;

            DeclareDependencies(ModuleDependency.Of(FlatLevelMapModule.ModuleId));
        }

        [ModuleFinalizer]
        protected void InitializeGlobalMapLoadingSystem(in ModuleInitializationParameter initParameter,
                                                        IModuleInitializer moduleInitializer)
        {
            moduleInitializer.Register(RegisterInitializeMapLoaderSystemId, 0, RegisterInitializeMapLoader);
        }

        void RegisterInitializeMapLoader(in ModuleInitializationParameter initParameter, IGameLoopSystemRegistration context)
        {
            if (initParameter.ServiceResolver.TryResolve(out IMapRegionMetaDataService<int> mds) &&
                mds is StaticMapLevelDataSource ds)
            {
                ds.Initialize();
            }
        }


    }
}
