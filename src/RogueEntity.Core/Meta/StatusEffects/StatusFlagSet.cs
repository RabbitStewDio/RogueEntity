using System;
using System.Collections;
using System.Collections.Generic;

namespace RogueEntity.Core.Meta.StatusEffects
{
    public readonly struct StatusFlagSet : IEquatable<StatusFlagSet>, IEnumerable<StatusFlag>
    {
        internal readonly long StatusEffects;
        readonly StatusFlagRegistry owner;

        public StatusFlagSet(StatusFlagRegistry owner, long statusEffects)
        {
            this.owner = owner;
            this.StatusEffects = statusEffects;
        }

        public StatusFlagSet Apply(StatusFlag e, bool v)
        {
            if (v)
            {
                return this + e;
            }

            return this - e;
        }

        public static StatusFlagSet operator+ (StatusFlagSet s, StatusFlag e)
        {
            if (e.Owner != s.owner)
            {
                throw new ArgumentException();
            }

            var statusEffects =  s.StatusEffects | 1L << e.LinearIndex;
            return new StatusFlagSet(s.owner, statusEffects);
        }

        public static StatusFlagSet operator- (StatusFlagSet s, StatusFlag e)
        {
            if (e.Owner != s.owner)
            {
                throw new ArgumentException();
            }

            var statusEffects = s.StatusEffects & (~(1L << e.LinearIndex));
            return new StatusFlagSet(s.owner, statusEffects);
        }

        public bool ContainsAll(StatusFlagQuery q)
        {
            return (StatusEffects & q.Mask) == q.Mask;
        }

        public bool ContainsAny(StatusFlagQuery q)
        {
            return (StatusEffects & q.Mask) != 0;
        }

        public bool ContainsNone(StatusFlagQuery q)
        {
            return (StatusEffects & q.Mask) == 0;
        }

        public bool Contains(StatusFlag e)
        {
            if (e.Owner != owner)
            {
                throw new ArgumentException();
            }
            
            var mask = 1L << e.LinearIndex;
            return (StatusEffects & mask) == mask;
        }

        public bool Equals(StatusFlagSet other)
        {
            return StatusEffects == other.StatusEffects && Equals(owner, other.owner);
        }

        public override bool Equals(object obj)
        {
            return obj is StatusFlagSet other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (StatusEffects.GetHashCode() * 397) ^ (owner != null ? owner.GetHashCode() : 0);
            }
        }

        public static bool operator ==(StatusFlagSet left, StatusFlagSet right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StatusFlagSet left, StatusFlagSet right)
        {
            return !left.Equals(right);
        }

        public struct Enumerator : IEnumerator<StatusFlag>
        {
            readonly long data;
            readonly StatusFlagRegistry registry;
            int position;

            public Enumerator(long data, StatusFlagRegistry registry) : this()
            {
                this.data = data;
                this.registry = registry;
                position = -1;
            }

            public bool MoveNext()
            {
                while (position + 1 < 64)
                {
                    position += 1;
                    var mask = 1 << position;
                    if ((data & mask) == mask && registry.TryGet(position, out var c))
                    {
                        Current = c;
                        return true;
                    }
                }

                return false;
            }

            public void Reset()
            {
                position = -1;
            }

            public void Dispose()
            {
            }

            public StatusFlag Current { get; private set; }

            object IEnumerator.Current => Current;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this.StatusEffects, this.owner);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<StatusFlag> IEnumerable<StatusFlag>.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}