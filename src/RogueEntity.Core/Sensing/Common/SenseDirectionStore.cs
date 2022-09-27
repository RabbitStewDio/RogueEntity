using System;
using System.Collections.Generic;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Common
{
    public readonly struct SenseDirectionStore : IEquatable<SenseDirectionStore>
    {
        static readonly Dictionary<(int x, int y), SenseDirection> directionMapping;
        static readonly Dictionary<SenseDirection, (int x, int y)> reverseDirectionMapping;
        static readonly bool[,] senseBlocking;
        const byte DirectionMask = 0xF;

        static SenseDirectionStore()
        {
            directionMapping = new Dictionary<(int, int), SenseDirection>
            {
                [(-1, -1)] = SenseDirection.North | SenseDirection.West,
                [(0, -1)] = SenseDirection.North,
                [(1, -1)] = SenseDirection.North | SenseDirection.East,

                [(-1, 0)] = SenseDirection.West,
                [(0, 0)] = SenseDirection.None,
                [(1, 0)] = SenseDirection.East,

                [(-1, 1)] = SenseDirection.South | SenseDirection.West,
                [(0, 1)] = SenseDirection.South,
                [(1, 1)] = SenseDirection.South | SenseDirection.East
            };

            reverseDirectionMapping = new Dictionary<SenseDirection, (int, int)>(9);
            foreach (var d in directionMapping)
            {
                reverseDirectionMapping[d.Value] = d.Key;
            }

            senseBlocking = new bool[16, 16];
            for (var a = 0; a < 16; a += 1)
            {
                for (var b = 0; b < 16; b += 1)
                {
                    if (!reverseDirectionMapping.TryGetValue((SenseDirection)a, out var da) ||
                        !reverseDirectionMapping.TryGetValue((SenseDirection)b, out var db))
                    {
                        senseBlocking[a, b] = false;
                        continue;
                    }

                    if (da.x == 0 && db.x == 0)
                    {
                        senseBlocking[a, b] = da.y != db.y;
                    }
                    else if (da.y == 0 && db.y == 0)
                    {
                        senseBlocking[a, b] = da.x != db.x;
                    }
                    else
                    {
                        senseBlocking[a, b] = da.x != db.x && da.y != db.y;
                    }
                }
            }
        }

        public static bool IsViewBlocked(SenseDirection a, SenseDirection b)
        {
            return senseBlocking[(int)a, (int)b];
        }
        
        public readonly byte RawData;

        public SenseDirectionStore(byte rawData)
        {
            this.RawData = rawData;
        }

        public SenseDirection Direction => (SenseDirection)(RawData & 0xf);
        public SenseDataFlags Flags => (SenseDataFlags)((RawData >> 4) & 0xf);
        public bool IsObstructed => (Flags & SenseDataFlags.Obstructed) == SenseDataFlags.Obstructed;

        public static SenseDirectionStore From(SenseDirection dir, SenseDataFlags flags)
        {
            var raw = (int)dir | ((int)flags << 4);
            return new SenseDirectionStore((byte)raw);
        }

        public static SenseDirectionStore From(Position2D pos) => From(pos.X, pos.Y);

        public static SenseDirectionStore From(int x, int y)
        {
            if (directionMapping.TryGetValue((Math.Sign(x), Math.Sign(y)), out var d))
            {
                return new SenseDirectionStore((byte)d);
            }

            return new SenseDirectionStore(0);
        }

        public SenseDirectionStore With(SenseDataFlags f)
        {
            var direction = (RawData & DirectionMask);
            var flags = (int)f << 4;
            return new SenseDirectionStore((byte)(direction | flags));
        }

        public SenseDirectionStore Merge(SenseDirectionStore other)
        {
            var d = Direction.Intersect(other.Direction);
            var f = Flags | other.Flags;
            return From(d, f);
        }

        public (int x, int y) ToDirectionalMovement()
        {
            return ToDirectionalMovement(Direction);
        }

        public static (int x, int y) ToDirectionalMovement(SenseDirection d)
        {
            if (reverseDirectionMapping.TryGetValue(d, out var result))
            {
                return result;
            }

            return (0, 0);
        }

        public override string ToString()
        {
            return $"{nameof(Direction)}: {Direction}, {nameof(Flags)}: {Flags}";
        }

        public bool Equals(SenseDirectionStore other)
        {
            return RawData == other.RawData;
        }

        public override bool Equals(object obj)
        {
            return obj is SenseDirectionStore other && Equals(other);
        }

        public override int GetHashCode()
        {
            return RawData.GetHashCode();
        }

        public static bool operator ==(SenseDirectionStore left, SenseDirectionStore right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SenseDirectionStore left, SenseDirectionStore right)
        {
            return !left.Equals(right);
        }
    }
}