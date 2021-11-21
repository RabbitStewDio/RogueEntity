using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.Players;

namespace RogueEntity.Core.MapLoading.MapRegions
{
    [Module]
    public class MapLoadingModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.MapLoading";

        public static readonly EntityRole ControlLevelLoadingRole = new EntityRole("Role.Core.MapLoading.ControlLevelLoading");

        public MapLoadingModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Map Loading";
            Description = "Provides base classes and behaviours for loading maps based on observer positions";
            IsFrameworkModule = true;

            DeclareDependency(ModuleDependency.Of(PlayerModule.ModuleId));
        }
    }
}
