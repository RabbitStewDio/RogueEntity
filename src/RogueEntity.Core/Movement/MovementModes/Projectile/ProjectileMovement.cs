using MessagePack;
using System.Runtime.Serialization;

namespace RogueEntity.Core.Movement.MovementModes.Projectile
{
    [MessagePackObject]
    [DataContract]
    public class ProjectileMovement: IMovementMode
    {
        public static readonly ProjectileMovement Instance = new ProjectileMovement();

        ProjectileMovement()
        {
        }
    }
}