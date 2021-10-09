using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.Positioning.SpatialQueries;

namespace RogueEntity.Core.Positioning
{
    [Module]
    public class PositionModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.Position";

        public static readonly EntitySystemId RegisterCommonPositions = "Entities.Core.Position";

        public static readonly EntitySystemId RegisterSpatialQuery = "Systems.Core.Position.RegisterSpatialQuery";

        public static readonly EntityRole PositionQueryRole = new EntityRole("Role.Core.Position.PositionQueryable");

        public static readonly EntityRole PositionedRole = new EntityRole("Role.Core.Position.Positionable");

        public PositionModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Positioning";
            Description = "Provides support for positioning items in a grid or continuous coordinate system";
            IsFrameworkModule = true;
        }

        [EntityRoleInitializer("Role.Core.Position.Positionable")]
        protected void InitializeCommon<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                  IModuleInitializer initializer,
                                                  EntityRole role)
            where TActorId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register(RegisterCommonPositions, 0, RegisterCommonEntities);

            if (!initParameter.ServiceResolver.TryResolve(out SpatialQueryRegistry r))
            {
                r = new SpatialQueryRegistry();
                initParameter.ServiceResolver.Store(r);
                initParameter.ServiceResolver.Store<ISpatialQueryLookup>(r);
            }
        }


        [EntityRoleFinalizerAttribute("Role.Core.Position.Positionable")]
        protected void FinalizePositionedRole<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                        IModuleInitializer initializer,
                                                        EntityRole role)
            where TActorId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TActorId>();
            ctx.Register(RegisterSpatialQuery, 9_999_999, RegisterSpatialQueryBruteForce);
        }

        void RegisterSpatialQueryBruteForce<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter, EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            if (!initParameter.ServiceResolver.TryResolve(out ISpatialQuery<TActorId> q))
            {
                q = new BruteForceSpatialQueryBackend<TActorId>(registry);
                initParameter.ServiceResolver.Store(q);

                if (initParameter.ServiceResolver.TryResolve(out SpatialQueryRegistry r))
                {
                    r.Register(q);
                }
            }
        }

        void RegisterCommonEntities<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                              EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            registry.RegisterNonConstructable<ImmobilityMarker>();
        }

    }
}
