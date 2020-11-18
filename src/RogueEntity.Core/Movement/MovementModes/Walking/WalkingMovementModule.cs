using RogueEntity.Api.Modules.Attributes;

namespace RogueEntity.Core.Movement.MovementModes.Walking
{
    [Module]
    public class WalkingMovementModule : MovementModuleBase<WalkingMovement>
    {
        public static readonly string ModuleId = "Core.Movement.Resistance.Walking";

        public WalkingMovementModule()
        {
            Id = ModuleId;

            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Movement Cost Modifier Source - Walking";
            Description = "Provides movement cost modifiers for walking movement";
            IsFrameworkModule = true;
        }
        
    }
}