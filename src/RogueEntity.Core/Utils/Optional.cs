using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Utils
{
    public static class Optional
    {
        public static Optional<T> Empty<T>()
        {
            return new Optional<T>(false, default);
        }

        public static Optional<T> ValueOf<T>(T value)
        {
            return new Optional<T>(true, value);
        }

        public static Optional<T> OfNullable<T>(T? value) where T: struct
        {
            if (!value.HasValue)
            {
                return Empty<T>();
            }

            return new Optional<T>(true, value.Value);
        }

        public static Optional<T> OfNullable<T>(T value) where T: class
        {
            if (value == null)
            {
                return Empty<T>();
            }

            return new Optional<T>(true, value);
        }
    }

    public readonly struct Optional<T> : IEquatable<Optional<T>>
    {
        readonly T value;

        internal Optional(bool hasValue, T value)
        {
            this.HasValue = hasValue;
            this.value = value;
        }

        public bool TryGetValue(out T value)
        {
            value = this.value;
            return HasValue;
        }

        public bool HasValue { get; }

        public override string ToString()
        {
            if (HasValue)
            {
                return $"Optional({nameof(value)}: {value})";
            }

            return "Optional<None>";
        }

        public static implicit operator Optional<T>(T data)
        {
            return Optional.ValueOf(data);
        }

        public bool Equals(Optional<T> other)
        {
            return HasValue == other.HasValue && EqualityComparer<T>.Default.Equals(value, other.value);
        }

        public bool Equals(T other)
        {
            return HasValue && EqualityComparer<T>.Default.Equals(value, other);
        }

        public override bool Equals(object obj)
        {
            return obj is Optional<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T>.Default.GetHashCode(value) * 397) ^ HasValue.GetHashCode();
            }
        }

        public static bool operator ==(Optional<T> left, T right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Optional<T> left, T right)
        {
            return !left.Equals(right);
        }

        public static bool operator ==(Optional<T> left, Optional<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Optional<T> left, Optional<T> right)
        {
            return !left.Equals(right);
        }
    }
}