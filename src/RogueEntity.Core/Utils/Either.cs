using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MessagePack;

namespace RogueEntity.Core.Utils
{
    /// <summary>
    ///   Avoid using Either as a serialized member. It is horrible to use properly during serialization.
    ///   To make it efficient, we would need to explicitly register surrogate providers/message-pack formatters
    ///   for each generic parameter combination. This is rather error prone and mad.
    /// </summary>
    /// <typeparam name="TA"></typeparam>
    /// <typeparam name="TB"></typeparam>
    [Serializable]
    [DataContract]
    [MessagePackObject]
    public readonly struct Either<TA, TB> : IEquatable<Either<TA, TB>>
    {
        [DataMember(Order = 0)]
        [Key(0)]
        readonly int state;

        [Key(1)]
        [DataMember(Order = 1)]
        public readonly TA Left;

        [Key(2)]
        [DataMember(Order = 2)]
        public readonly TB Right;

        [SerializationConstructor]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members",
                                                         Justification = "Used implicitly by the serializer")]
        Either(int state, TA left, TB right)
        {
            this.state = state;
            Left = left;
            Right = right;
        }

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
        public static Either<TA, TB> Of<TA, TB>(in TA a) => new Either<TA, TB>(in a);
        public static Either<TA, TB> Of<TA, TB>(in TB b) => new Either<TA, TB>(in b);
    }
}