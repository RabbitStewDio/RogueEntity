using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Positioning.SpatialQueries
{
    public readonly struct SpatialQueryResult<TEntityId, TComponent> : IEquatable<SpatialQueryResult<TEntityId, TComponent>>
    {
        sealed class DistanceRelationalComparer : IComparer<SpatialQueryResult<TEntityId, TComponent>>
        {
            public int Compare(SpatialQueryResult<TEntityId, TComponent> x, SpatialQueryResult<TEntityId, TComponent> y)
            {
                return x.Distance.CompareTo(y.Distance);
            }
        }

        public static IComparer<SpatialQueryResult<TEntityId, TComponent>> DistanceComparer { get; } = new DistanceRelationalComparer();

        static readonly EqualityComparer<TEntityId> entityComparer = EqualityComparer<TEntityId>.Default;
        static readonly EqualityComparer<TComponent> componentComparer = EqualityComparer<TComponent>.Default;

        public readonly TEntityId EntityId;
        public readonly Position Position;
        public readonly TComponent Component;
        public readonly float Distance; // not needed to be compared for equality.

        public SpatialQueryResult(TEntityId entityId, Position position, TComponent component, float distance)
        {
            EntityId = entityId;
            Position = position;
            Component = component;
            Distance = distance;
        }

        public bool Equals(SpatialQueryResult<TEntityId, TComponent> other)
        {
            return entityComparer.Equals(EntityId, other.EntityId) &&
                   Position.Equals(other.Position) &&
                   componentComparer.Equals(Component, other.Component);
        }

        public override bool Equals(object obj)
        {
            return obj is SpatialQueryResult<TEntityId, TComponent> other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = entityComparer.GetHashCode(EntityId);
                hashCode = (hashCode * 397) ^ Position.GetHashCode();
                hashCode = (hashCode * 397) ^ componentComparer.GetHashCode(Component);
                return hashCode;
            }
        }

        public static bool operator ==(SpatialQueryResult<TEntityId, TComponent> left, SpatialQueryResult<TEntityId, TComponent> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SpatialQueryResult<TEntityId, TComponent> left, SpatialQueryResult<TEntityId, TComponent> right)
        {
            return !left.Equals(right);
        }
    }
}