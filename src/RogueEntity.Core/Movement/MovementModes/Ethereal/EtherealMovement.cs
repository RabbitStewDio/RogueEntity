using MessagePack;
using System.Runtime.Serialization;

namespace RogueEntity.Core.Movement.MovementModes.Ethereal
{
    [MessagePackObject]
    [DataContract]
    public class EtherealMovement : IMovementMode
    {
        public static readonly EtherealMovement Instance = new EtherealMovement();

        EtherealMovement()
        {
        }

        public bool Equals(IMovementMode? other)
        {
            return Equals((object?)other);
        }

        public bool Equals(EtherealMovement? other)
        {
            return other != null;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((EtherealMovement)obj);
        }

        public override int GetHashCode()
        {
            return nameof(EtherealMovement).GetHashCode();
        }

        public static bool operator ==(EtherealMovement? left, EtherealMovement? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EtherealMovement? left, EtherealMovement? right)
        {
            return !Equals(left, right);
        }
    }
}