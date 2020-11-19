namespace RogueEntity.Core.Movement.MovementModes.Swimming
{
    public class SwimmingMovementModule : MovementModuleBase<SwimmingMovement>
    {
        public static readonly string ModuleId = "Core.Movement.Resistance.SwimmingMovement";

        public SwimmingMovementModule()
        {
            Id = ModuleId;

            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Movement Cost Modifier Source - Swimming";
            Description = "Provides movement cost modifiers for swimming movement in or through water";
            IsFrameworkModule = true;
        }
    }
}
