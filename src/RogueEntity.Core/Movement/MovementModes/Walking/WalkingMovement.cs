using MessagePack;
using System.Runtime.Serialization;

namespace RogueEntity.Core.Movement.MovementModes.Walking
{
    [MessagePackObject]
    [DataContract]
    public class WalkingMovement: IMovementMode
    {
        public static readonly WalkingMovement Instance = new WalkingMovement();
        
        WalkingMovement()
        {
        }
    }
}