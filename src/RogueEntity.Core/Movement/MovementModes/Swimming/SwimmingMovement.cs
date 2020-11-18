namespace RogueEntity.Core.Movement.MovementModes.Swimming
{
    public class SwimmingMovement: IMovementMode
    {
        public static readonly SwimmingMovement Instance = new SwimmingMovement();
        
        SwimmingMovement()
        {
        }
    }
}