using RogueEntity.Api.Modules.Attributes;

namespace RogueEntity.Core.Movement.MovementModes.Flying
{
    [Module]
    public class FlyingMovementModule : MovementModuleBase<FlyingMovement>
    {
        public static readonly string ModuleId = "Core.Movement.Resistance.FlyingMovement";

        public FlyingMovementModule()
        {
            Id = ModuleId;

            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Movement Cost Modifier Source - Flying";
            Description = "Provides movement cost modifiers for flying movement through the air";
            IsFrameworkModule = true;
        }

        protected override FlyingMovement GetMovementModeInstance()
        {
            return FlyingMovement.Instance;
        }
    }
}