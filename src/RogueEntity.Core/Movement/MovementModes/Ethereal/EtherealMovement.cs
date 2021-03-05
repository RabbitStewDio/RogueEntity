using MessagePack;
using System.Runtime.Serialization;

namespace RogueEntity.Core.Movement.MovementModes.Ethereal
{
    [MessagePackObject]
    [DataContract]
    public class EtherealMovement: IMovementMode
    {
        public static readonly EtherealMovement Instance = new EtherealMovement();
        
        EtherealMovement()
        {
        }
    }
}