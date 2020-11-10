namespace RogueEntity.Core.Movement.CostModifier
{
    public class WalkingMovementCostModifierModule : MovementCostModifierModuleBase<WalkingMovement>
    {
        public static readonly string ModuleId = "Core.Movement.Resistance.Walking";

        public WalkingMovementCostModifierModule()
        {
            Id = ModuleId;

            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Movement Cost Modifier Source - Walking";
            Description = "Provides movement cost modifiers for walking movement";
            IsFrameworkModule = true;
        }
        
    }
}