using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Utils
{
    public readonly struct Either<TA, TB> : IEquatable<Either<TA, TB>>
    {
        readonly int state;
        public readonly TA Left;
        public readonly TB Right;

        public Either(in TA a)
        {
            state = 1;
            Left = a;
            Right = default;
        }

        public Either(in TB b)
        {
            state = 2;
            Left = default;
            Right = b;
        }

        public bool TryGet(out TA a)
        {
            if (state == 1)
            {
                a = Left;
                return true;
            }

            a = default;
            return false;
        }

        public bool TryGet(out TB a)
        {
            if (state == 2)
            {
                a = Right;
                return true;
            }

            a = default;
            return false;
        }

        public override string ToString()
        {
            if (TryGet(out TA a))
            {
                return $"{a}";
            }
            if (TryGet(out TB b))
            {
                return $"{b}";
            }
            return $"<!>";
        }

        public bool Equals(Either<TA, TB> other)
        {
            return state == other.state && EqualityComparer<TA>.Default.Equals(Left, other.Left) && EqualityComparer<TB>.Default.Equals(Right, other.Right);
        }

        public override bool Equals(object obj)
        {
            return obj is Either<TA, TB> other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = state;
                hashCode = (hashCode * 397) ^ EqualityComparer<TA>.Default.GetHashCode(Left);
                hashCode = (hashCode * 397) ^ EqualityComparer<TB>.Default.GetHashCode(Right);
                return hashCode;
            }
        }

        public static bool operator ==(Either<TA, TB> left, Either<TA, TB> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Either<TA, TB> left, Either<TA, TB> right)
        {
            return !left.Equals(right);
        }
    }

    public static class Either
    {
        public static Either<TA,TB> Of<TA,TB>(in TA a) => new Either<TA, TB>(in a);
        public static Either<TA,TB> Of<TA,TB>(in TB b) => new Either<TA, TB>(in b);
    }
}
