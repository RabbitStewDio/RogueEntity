using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;
using MessagePack;

namespace RogueEntity.Core.Utils.DataViews
{
    [DataContract]
    [MessagePackObject]
    public class BoundedBoolDataView: IReadOnlyBoundedBoolDataView, IBoundedBoolDataViewRawAccess, IEquatable<BoundedBoolDataView>
    {
        [Key(0)]
        [DataMember(Order = 0)]
        Rectangle bounds;
        [Key(1)]
        [DataMember(Order = 1)]
        byte[] data;

        [Key(2)]
        [DataMember(Order = 2)]
        int anyValueSet;

        const int WordSize = 8;
        
        public BoundedBoolDataView(in Rectangle bounds)
        {
            this.bounds = bounds;
            var size = Align(bounds.Width * bounds.Height, WordSize);
            data = new byte[size];
            anyValueSet = 0;
        }

        [SerializationConstructor]
        protected internal BoundedBoolDataView(Rectangle bounds, byte[] data, int anyValueSet)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var size = Align(bounds.Width * bounds.Height, WordSize);
            if (data.Length != size)
            {
                throw new ArgumentException();
            }
            
            this.bounds = bounds;
            this.data = data;
            this.anyValueSet = anyValueSet;
        }

        [IgnoreDataMember]
        [IgnoreMember]
        public Rectangle Bounds => bounds;

        [IgnoreDataMember]
        [IgnoreMember]
        public byte[] Data => data;

        public void Resize(in Rectangle newBounds)
        {
            if (bounds.Width == newBounds.Width &&
                bounds.Height == newBounds.Height)
            {
                return;
            }

            var size = Align(bounds.Width * bounds.Height, WordSize);
            if (bounds.Width >= newBounds.Width &&
                bounds.Height >= newBounds.Height)
            {
                // rebase origin, but dont change coordinates.
                this.bounds = new Rectangle(newBounds.X, newBounds.Y, bounds.Width, bounds.Height);
                Array.Clear(data, 0, data.Length);
                this.anyValueSet = 0;
            }
            else
            {
                this.bounds = newBounds;
                this.data = new byte[size];
                this.anyValueSet = 0;
            }
        }

        static int Align(int v, int boundary)
        {
            return (int)Math.Ceiling(v / (float)boundary) * boundary;
        }

        public bool Contains(int x, int y)
        {
            return bounds.Contains(x, y);
        }

        bool TryGetRawIndex(int x, int y, out int result)
        {
            if (bounds.Contains(x, y))
            {
                var rawX = x - bounds.X;
                var rawY = y - bounds.Y;
                result = (rawX + rawY * bounds.Width);
                return true;
            }

            result = default;
            return false;
        }

        public ref bool TryGetForUpdate(int x, int y, ref bool defaultValue, out bool success)
        {
            throw new InvalidOperationException();
        }

        public bool Any(int x, int y)
        {
            if (TryGetRawIndex(x, y, out var rawBitIdx))
            {
                var chunkIndex = Math.DivRem(rawBitIdx, WordSize, out _);
                var chunk = data[chunkIndex];
                return chunk != 0;
            }

            return false;
        }

        public bool TryGet(int x, int y, out bool result)
        {
            if (!TryGetRawIndex(x, y, out var rawBitIdx))
            {
                result = default;
                return false;
            }
            
            var chunkIndex = Math.DivRem(rawBitIdx, WordSize, out var innerIndex);
            var chunk = data[chunkIndex];
            if (chunk == 0)
            {
                result = false;
                return true;
            }

            if (chunk == byte.MaxValue)
            {
                result = true;
                return true;
            }

            var bitMask = 1u << innerIndex;
            result = (chunk & bitMask) == bitMask;
            return true;
        }

        public bool TrySet(int x, int y, in bool value)
        {
            if (!TryGetRawIndex(x, y, out var rawBitIdx))
            {
                return false;
            }
                
            var chunkIndex = Math.DivRem(rawBitIdx, WordSize, out var innerIndex);
            var bitMask = (byte) (1u << innerIndex);
            var oldV = this.data[chunkIndex];
            int newV;
            if (value)
            {
                newV = oldV | bitMask;
            }
            else
            {
                newV = data[chunkIndex] & ~bitMask;
            }
                
            data[chunkIndex] = (byte) newV;
            anyValueSet += Math.Sign(newV - oldV);
            return true;
        }

        public bool this[int x, int y]
        {
            get
            {
                if (!TryGetRawIndex(x, y, out var rawBitIdx))
                {
                    return false;
                }
                
                var chunkIndex = Math.DivRem(rawBitIdx, WordSize, out var innerIndex);
                var chunk = data[chunkIndex];
                if (chunk == 0) return false;
                if (chunk == byte.MaxValue) return true;

                var bitMask = 1u << innerIndex;
                return (chunk & bitMask) == bitMask;
            }
            set
            {
                TrySet(x, y, in value);
            }
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            for (int y = Bounds.X; y < Bounds.MaxExtentY; y += 1)
            {
                for (int x = Bounds.Y; x < Bounds.MaxExtentX; x += 1)
                {
                    b.Append(this[x, y] ? '*' : '.');
                }

                b.Append('\n');
            }

            return b.ToString();
        }

        public void Clear()
        {
            Array.Clear(data, 0, data.Length);
        }

        public void Fill(in bool b)
        {
            byte val = b ? byte.MaxValue : byte.MinValue;
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = val;
            }

            anyValueSet = b ? data.Length : 0;
        }

        public bool AnyValueSet()
        {
            return anyValueSet != 0;
        }

        public bool Equals(BoundedBoolDataView other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return bounds.Equals(other.bounds) && data.Equals(other.data);
        }

        public override bool Equals(object obj)
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

            return Equals((BoundedBoolDataView) obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            return bounds.GetHashCode();
        }

        public static bool operator ==(BoundedBoolDataView left, BoundedBoolDataView right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BoundedBoolDataView left, BoundedBoolDataView right)
        {
            return !Equals(left, right);
        }
    }
}