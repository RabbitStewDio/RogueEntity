using RogueEntity.Api.Modules.Attributes;

namespace RogueEntity.Core.Movement.MovementModes.Projectile
{
    [Module]
    public class ProjectileMovementModule: MovementModuleBase<ProjectileMovement>
    {
        public static readonly string ModuleId = "Core.Movement.Resistance.ProjectileMovement";

        public ProjectileMovementModule()
        {
            Id = ModuleId;

            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Movement Cost Modifier Source - Projectiles";
            Description = "Provides movement cost modifiers for projectiles flying through the air";
            IsFrameworkModule = true;
        }

        protected override ProjectileMovement GetMovementModeInstance()
        {
            return ProjectileMovement.Instance;
        }
    }
}