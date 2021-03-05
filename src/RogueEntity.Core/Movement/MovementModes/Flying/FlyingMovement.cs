using MessagePack;
using System.Runtime.Serialization;

namespace RogueEntity.Core.Movement.MovementModes.Flying
{
    [MessagePackObject]
    [DataContract]
    public class FlyingMovement: IMovementMode
    {
        public static readonly FlyingMovement Instance = new FlyingMovement();
        
        FlyingMovement()
        {
        }
    }
}