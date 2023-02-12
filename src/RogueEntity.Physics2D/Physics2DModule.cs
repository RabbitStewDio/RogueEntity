using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Continuous;

namespace RogueEntity.Physics2D;

public class Physics2DModule: ModuleBase
{
    public static readonly EntityRole Physics2DEntityRole = new EntityRole("Role.Core.Physics2D.Entity");
    public static readonly EntityRole StaticPhysics2DEntityRole = new EntityRole("Role.Core.Physics2D.Entity.Static");
    public static readonly EntityRole DynamicPhysics2DEntityRole = new EntityRole("Role.Core.Physics2D.Entity.Static");
    public static readonly EntityRelation CollidesWithRelation = new EntityRelation("Relation.Core.Physics2D.CollidesWith", Physics2DEntityRole, Physics2DEntityRole);

    public Physics2DModule()
    {
        Id = "Core.Physics2D";
        Author = "RogueEntity.Core";
        Name = "RogueEntity Core Module - Physics2D";
        Description = "Provides base classes for 2D physics implementations";
        IsFrameworkModule = true;

        RequireRole(Physics2DEntityRole);
        RequireRole(StaticPhysics2DEntityRole).WithImpliedRole(Physics2DEntityRole).WithRequiredRole(PositionModule.PositionedRole);
        RequireRole(DynamicPhysics2DEntityRole).WithImpliedRole(Physics2DEntityRole).WithRequiredRole(ContinuousPositionModule.ContinuousPositionedRole);
        RequireRelation(CollidesWithRelation);
    }
}