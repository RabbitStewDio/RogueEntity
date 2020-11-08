using System;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public readonly struct EntityRole : IEquatable<EntityRole>
    {
        public readonly string Id;

        public EntityRole(string id)
        {
            Id = id;
        }

        public bool Equals(EntityRole other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityRole other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }

        public static bool operator ==(EntityRole left, EntityRole right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityRole left, EntityRole right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"Role({Id})";
        }
    }

    public readonly struct EntityRelation : IEquatable<EntityRelation>
    {
        public readonly string Id;
        public readonly EntityRole Subject;
        public readonly EntityRole Object;
        public readonly bool Optional;

        public EntityRelation(string id, EntityRole subject, EntityRole o, bool optional = false)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Subject = subject;
            Object = o;
            Optional = optional;
        }

        public bool Equals(EntityRelation other)
        {
            return Id == other.Id && Subject.Equals(other.Subject) && Object.Equals(other.Object) && Optional == other.Optional;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityRelation other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ Subject.GetHashCode();
                hashCode = (hashCode * 397) ^ Object.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(EntityRelation left, EntityRelation right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityRelation left, EntityRelation right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            var optional = Optional ? ", Optional" : "";
            return $"Relation({nameof(Id)}: {Id}, {nameof(Subject)}: {Subject}, {nameof(Object)}: {Object}{optional})";
        }
    }
}