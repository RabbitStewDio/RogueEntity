using System;

namespace RogueEntity.Core.Infrastructure.ItemTraits
{
    public static class EntityRelationInstanceExtensions
    {
        public static EntityRelationInstance Instantiate<TSubjectType, TObjectType>(this EntityRelation r)
        {
            return new EntityRelationInstance(r, typeof(TSubjectType), typeof(TObjectType));
        }
    }
    
    public readonly struct EntityRelationInstance : IEquatable<EntityRelationInstance>
    {
        public EntityRelation Relation { get; }
        public Type SubjectEntityType { get; }
        public Type ObjectEntityType { get; }

        public EntityRelationInstance(EntityRelation relation, Type subjectEntityType, Type objectEntityType)
        {
            this.Relation = relation;
            this.SubjectEntityType = subjectEntityType;
            this.ObjectEntityType = objectEntityType;
        }

        public bool Equals(EntityRelationInstance other)
        {
            return Relation.Equals(other.Relation) && SubjectEntityType == other.SubjectEntityType && ObjectEntityType == other.ObjectEntityType;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityRelationInstance other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Relation.GetHashCode();
                hashCode = (hashCode * 397) ^ (SubjectEntityType != null ? SubjectEntityType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ObjectEntityType != null ? ObjectEntityType.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(EntityRelationInstance left, EntityRelationInstance right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityRelationInstance left, EntityRelationInstance right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"{nameof(Relation)}(\'{Relation.Id}\', Subject: ({SubjectEntityType}, {Relation.Subject.Id}), Target: ({ObjectEntityType}, {Relation.Object.Id}))";
        }
    }
}