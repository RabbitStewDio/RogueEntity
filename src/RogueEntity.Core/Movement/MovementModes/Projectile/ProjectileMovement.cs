using MessagePack;
using System;
using System.Runtime.Serialization;

namespace RogueEntity.Core.Movement.MovementModes.Projectile
{
    [MessagePackObject]
    [DataContract]
    public class ProjectileMovement: IMovementMode, IEquatable<ProjectileMovement>
    {
        public static readonly ProjectileMovement Instance = new ProjectileMovement();

        ProjectileMovement()
        {
        }

        public bool Equals(IMovementMode? other)
        {
            return Equals((object?) other);
        }

        public bool Equals(ProjectileMovement? other)
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

            return Equals((ProjectileMovement)obj);
        }

        public override int GetHashCode()
        {
            return nameof(ProjectileMovement).GetHashCode();
        }

        public static bool operator ==(ProjectileMovement? left, ProjectileMovement? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProjectileMovement? left, ProjectileMovement? right)
        {
            return !Equals(left, right);
        }
    }
}