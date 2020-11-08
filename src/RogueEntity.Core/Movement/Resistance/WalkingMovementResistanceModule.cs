namespace RogueEntity.Core.Movement.Resistance
{
    public class WalkingMovementResistanceModule : MovementResistanceModule<WalkingMovement>
    {
        public static readonly string ModuleId = "Core.Movement.Resistance.Walking";

        public WalkingMovementResistanceModule()
        {
            Id = ModuleId;
        }
    }
}