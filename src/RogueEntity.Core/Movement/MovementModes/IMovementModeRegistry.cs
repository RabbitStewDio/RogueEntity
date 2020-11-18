namespace RogueEntity.Core.Movement.MovementModes
{
    public interface IMovementModeRegistry
    {
        public int GetLinearIndex<TMovementMode>();
    }
    
    
}