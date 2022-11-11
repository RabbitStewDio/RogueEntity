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

        public bool Equals(IMovementMode? other)
        {
            return Equals((object?)other);
        }

        public bool Equals(SwimmingMovement? other)
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

            return Equals((SwimmingMovement)obj);
        }

        public override int GetHashCode()
        {
            return nameof(SwimmingMovement).GetHashCode();
        }

        public static bool operator ==(SwimmingMovement? left, SwimmingMovement? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SwimmingMovement? left, SwimmingMovement? right)
        {
            return !Equals(left, right);
        }
    }
}