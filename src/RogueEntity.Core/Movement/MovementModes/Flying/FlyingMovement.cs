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
        

        public bool Equals(IMovementMode? other)
        {
            return Equals((object?)other);
        }

        public bool Equals(FlyingMovement? other)
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

            return Equals((FlyingMovement)obj);
        }

        public override int GetHashCode()
        {
            return nameof(FlyingMovement).GetHashCode();
        }

        public static bool operator ==(FlyingMovement? left, FlyingMovement? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FlyingMovement? left, FlyingMovement? right)
        {
            return !Equals(left, right);
        }
    }
}