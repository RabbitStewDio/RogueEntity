namespace RogueEntity.Core.Movement.MovementModes.Projectile
{
    public class ProjectileMovement: IMovementMode
    {
        public static readonly ProjectileMovement Instance = new ProjectileMovement();

        ProjectileMovement()
        {
        }
    }
}