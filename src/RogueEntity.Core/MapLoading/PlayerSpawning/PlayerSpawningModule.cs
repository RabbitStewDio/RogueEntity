using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Players;

namespace RogueEntity.Core.MapLoading.PlayerSpawning
{
    public class PlayerSpawningModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.PlayerSpawning";

        public static readonly EntityRole PlayerSpawnPointRole = new EntityRole("Role.Core.PlayerSpawning.PlayerSpawnPoint");
        public static readonly EntityRelation PlayerToSpawnPointRelation = new EntityRelation("Relation.Core.PlayerSpawning.PlayerToSpawnPoint", PlayerModule.PlayerRole, PlayerSpawnPointRole, true);

        static readonly EntitySystemId playerSpawnPointComponentsId = "Entities.Core.PlayerSpawning.PlayerSpawnPoint";

        public PlayerSpawningModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Player Spawning";
            Description = "Provides base classes and behaviours for spawning players into a map";
            IsFrameworkModule = true;

            DeclareDependencies(ModuleDependency.Of(PlayerModule.ModuleId),
                                ModuleDependency.Of(MapLoadingModule.ModuleId));

            RequireRole(PlayerSpawnPointRole);
            RequireRelation(PlayerToSpawnPointRelation).WithImpliedRole(MapLoadingModule.ControlLevelLoadingRole);
        }

        [EntityRoleInitializer("Role.Core.PlayerSpawning.PlayerSpawnPoint")]
        protected void InitializePlayerSpawnPointRole<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                               IModuleInitializer initializer,
                                                               EntityRole r)
            where TItemId : struct, IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(playerSpawnPointComponentsId, -20_000, RegisterPlayerSpawnPointComponents);
        }

        void RegisterPlayerSpawnPointComponents<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                         EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            registry.RegisterNonConstructable<PlayerSpawnLocation>();
        }
    }
}
