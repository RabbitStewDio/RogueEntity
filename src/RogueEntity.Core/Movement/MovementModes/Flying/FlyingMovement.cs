namespace RogueEntity.Core.Movement.MovementModes.Flying
{
    public class FlyingMovement: IMovementMode
    {
        public static readonly FlyingMovement Instance = new FlyingMovement();
        
        FlyingMovement()
        {
        }
    }
}