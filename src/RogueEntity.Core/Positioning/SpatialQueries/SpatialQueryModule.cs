using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;

namespace RogueEntity.Core.Positioning.SpatialQueries;

public class SpatialQueryModule : ModuleBase
{
    public static readonly string ModuleId = "Core.Position.SpatialQueries";

    public static readonly EntitySystemId RegisterCommonPositionsEntitySystemId = "Entities.Core.Position";
    public static readonly EntitySystemId RegisterSpatialQuerySystemId = "Systems.Core.Position.RegisterSpatialQuery";
    public static readonly EntitySystemId RegisterResetMapDataSystemId = "Systems.Core.Position.RegisterResetMapData";

    public static readonly EntityRole PositionQueryRole = new EntityRole("Role.Core.Position.PositionQueryable");
    public static readonly EntityRole PositionedRole = new EntityRole("Role.Core.Position.Positionable");

    public SpatialQueryModule()
    {
        Id = ModuleId;
        Author = "RogueEntity.Core";
        Name = "RogueEntity Core Module - Spatial Queries";
        Description = "Provides support for effectively querying reference items";
        IsFrameworkModule = true;
    }

    [EntityRoleInitializer("Role.Core.Position.PositionQueryable")]
    protected void InitializeGridPositioned<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                      IModuleInitializer initializer,
                                                      EntityRole role)
        where TActorId : struct, IEntityKey
    {
        
    }

}