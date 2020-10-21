using System;
using System.Runtime.Serialization;
using System.Text;
using GoRogue;
using JetBrains.Annotations;
using MessagePack;

namespace RogueEntity.Core.Utils.Maps
{
    public interface IBoundedBoolDataViewRawAccess
    {
        Rectangle Bounds { get; }
        byte[] Data { get; }
    }
    
    [DataContract]
    [MessagePackObject]
    public class BoundedBoolDataView: IReadOnlyView2D<bool>
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
        internal BoundedBoolDataView(Rectangle bounds, [NotNull] byte[] data, int anyValueSet)
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
                this.bounds = new Rectangle(newBounds.MinExtentX, newBounds.MinExtentY, bounds.Width, bounds.Height);
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

        bool TryGetRawIndex(int x, int y, out int result)
        {
            if (bounds.Contains(x, y))
            {
                var rawX = x - bounds.MinExtentX;
                var rawY = y - bounds.MinExtentY;
                result = (rawX + rawY * bounds.Width);
                return true;
            }

            result = default;
            return false;
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
                if (!TryGetRawIndex(x, y, out var rawBitIdx))
                {
                    return;
                }
                
                var chunkIndex = Math.DivRem(rawBitIdx, WordSize, out var innerIndex);
                var bitMask = (byte) (1u << innerIndex);
                var oldV = data[chunkIndex];
                var newV = 0;
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
            }
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            for (int y = Bounds.MinExtentY; y < Bounds.MaxExtentY; y += 1)
            {
                for (int x = Bounds.MinExtentX; x < Bounds.MaxExtentX; x += 1)
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


        public bool AnyValueSet()
        {
            return anyValueSet != 0;
        }
    }
}