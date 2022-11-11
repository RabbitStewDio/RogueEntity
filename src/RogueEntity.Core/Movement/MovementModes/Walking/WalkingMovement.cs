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

        public bool Equals(IMovementMode? other)
        {
            return Equals((object?)other);
        }

        public bool Equals(WalkingMovement? other)
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

            return Equals((WalkingMovement)obj);
        }

        public override int GetHashCode()
        {
            return nameof(WalkingMovement).GetHashCode();
        }

        public static bool operator ==(WalkingMovement? left, WalkingMovement? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(WalkingMovement? left, WalkingMovement? right)
        {
            return !Equals(left, right);
        }
    }
}