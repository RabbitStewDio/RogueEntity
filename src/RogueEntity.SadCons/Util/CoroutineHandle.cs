using System;

namespace RogueEntity.SadCons.Util
{
    public readonly struct CoroutineHandle : IEquatable<CoroutineHandle>
    {
        readonly Guid guid;

        CoroutineHandle(Guid guid)
        {
            this.guid = guid;
        }

        public static CoroutineHandle Create()
        {
            return new CoroutineHandle(Guid.NewGuid());
        }

        public bool Equals(CoroutineHandle other)
        {
            return guid.Equals(other.guid);
        }

        public override bool Equals(object obj)
        {
            return obj is CoroutineHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return guid.GetHashCode();
        }

        public static bool operator ==(CoroutineHandle left, CoroutineHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CoroutineHandle left, CoroutineHandle right)
        {
            return !left.Equals(right);
        }
    }
}
