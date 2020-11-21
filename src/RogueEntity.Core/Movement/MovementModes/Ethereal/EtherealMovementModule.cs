using RogueEntity.Api.Modules.Attributes;

namespace RogueEntity.Core.Movement.MovementModes.Ethereal
{
    [Module]
    public class EtherealMovementModule: MovementModuleBase<EtherealMovement>
    {
        public static readonly string ModuleId = "Core.Movement.Resistance.EtherealMovement";

        public EtherealMovementModule()
        {
            Id = ModuleId;

            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Movement Cost Modifier Source - Ethereal";
            Description = "Provides movement cost modifiers for ghostly movement that passes through solid materials";
            IsFrameworkModule = true;
        }

        protected override EtherealMovement GetMovementModeInstance()
        {
            return EtherealMovement.Instance;
        }
    }
}