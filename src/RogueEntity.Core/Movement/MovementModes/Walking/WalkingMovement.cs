namespace RogueEntity.Core.Movement.MovementModes.Walking
{
    public class WalkingMovement: IMovementMode
    {
        public static readonly WalkingMovement Instance = new WalkingMovement();
        
        WalkingMovement()
        {
        }
    }
}