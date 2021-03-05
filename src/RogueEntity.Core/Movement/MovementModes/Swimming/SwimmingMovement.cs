using MessagePack;
using System.Runtime.Serialization;

namespace RogueEntity.Core.Movement.MovementModes.Swimming
{
    [MessagePackObject]
    [DataContract]
    public class SwimmingMovement: IMovementMode
    {
        public static readonly SwimmingMovement Instance = new SwimmingMovement();
        
        SwimmingMovement()
        {
        }
    }
}