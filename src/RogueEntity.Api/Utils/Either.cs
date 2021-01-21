using System;
using System.Collections.Generic;

namespace RogueEntity.Api.Utils
{
    public readonly struct EitherLeft<TLeft> : IEquatable<EitherLeft<TLeft>>
    {
        public readonly TLeft Value;

        public EitherLeft(TLeft value)
        {
            Value = value;
        }

        public bool Equals(EitherLeft<TLeft> other)
        {
            return EqualityComparer<TLeft>.Default.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is EitherLeft<TLeft> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<TLeft>.Default.GetHashCode(Value);
        }

        public static bool operator ==(EitherLeft<TLeft> left, EitherLeft<TLeft> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EitherLeft<TLeft> left, EitherLeft<TLeft> right)
        {
            return !left.Equals(right);
        }
    }
    
    public readonly struct EitherRight<TRight> : IEquatable<EitherRight<TRight>>
    {
        public readonly TRight Value;

        public EitherRight(TRight value)
        {
            Value = value;
        }

        public bool Equals(EitherRight<TRight> other)
        {
            return EqualityComparer<TRight>.Default.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is EitherRight<TRight> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<TRight>.Default.GetHashCode(Value);
        }

        public static bool operator ==(EitherRight<TRight> left, EitherRight<TRight> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EitherRight<TRight> left, EitherRight<TRight> right)
        {
            return !left.Equals(right);
        }
    }
    
    public readonly struct Either<TLeft, TRight>
    {
        readonly byte state;
        public readonly TLeft Left;
        public readonly TRight Right;

        internal Either(TLeft left)
        {
            this.state = 1;
            this.Left = left;
            this.Right = default;
        }

        internal Either(TRight right)
        {
            this.state = 2;
            this.Left = default;
            this.Right = right;
        }

        internal Either(byte state, TLeft left, TRight right)
        {
            this.state = state;
            this.Left = left;
            this.Right = right;
        }

        public Optional<TLeft> LeftOption => state == 1 ? Optional.ValueOf(Left) : Optional.Empty();
        public Optional<TRight> RightOption => state == 2 ? Optional.ValueOf(Right) : Optional.Empty();

        public bool HasLeft => state == 1;
        public bool HasRight => state == 2;

        public static implicit operator TLeft(Either<TLeft, TRight> e)
        {
            return e.Left;
        }

        public static implicit operator TRight(Either<TLeft, TRight> e)
        {
            return e.Right;
        }

        public static implicit operator EitherLeft<TLeft>(Either<TLeft, TRight> e)
        {
            return new EitherLeft<TLeft>(e.Left);
        }

        public static implicit operator EitherRight<TRight>(Either<TLeft, TRight> e)
        {
            return new EitherRight<TRight>(e.Right);
        }

        public static implicit operator Either<TLeft, TRight>(TLeft e)
        {
            return new Either<TLeft, TRight>(1, e, default);
        }

        public static implicit operator Either<TLeft, TRight>(TRight e)
        {
            return new Either<TLeft, TRight>(2, default, e);
        }

        public IEnumerable<TResult> Select<TResult>(Func<TLeft, TResult> fn)
        {
            if (state == 1)
            {
                yield return fn(Left);
            }
        }
        
        public IEnumerable<TResult> Select<TResult>(Func<TRight, TResult> fn)
        {
            if (state == 2)
            {
                yield return fn(Right);
            }
        }
    }
}
