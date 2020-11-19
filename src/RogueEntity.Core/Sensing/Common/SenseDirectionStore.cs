using System;
using System.Collections.Generic;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Common
{
    public readonly struct SenseDirectionStore
    {
        static readonly Dictionary<(int x, int y), SenseDirection> DirectionMapping;
        static readonly Dictionary<SenseDirection, (int x, int y)> ReverseDirectionMapping;
        static readonly bool[,] SenseBlocking;
        const byte DirectionMask = 0xF;

        static SenseDirectionStore()
        {
            DirectionMapping = new Dictionary<(int, int), SenseDirection>
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

            ReverseDirectionMapping = new Dictionary<SenseDirection, (int, int)>(9);
            foreach (var d in DirectionMapping)
            {
                ReverseDirectionMapping[d.Value] = d.Key;
            }

            SenseBlocking = new bool[16, 16];
            for (var a = 0; a < 16; a += 1)
            {
                for (var b = 0; b < 16; b += 1)
                {
                    if (!ReverseDirectionMapping.TryGetValue((SenseDirection)a, out var da) ||
                        !ReverseDirectionMapping.TryGetValue((SenseDirection)b, out var db))
                    {
                        SenseBlocking[a, b] = false;
                        continue;
                    }

                    if (da.x == 0 && db.x == 0)
                    {
                        SenseBlocking[a, b] = da.y != db.y;
                    }
                    else if (da.y == 0 && db.y == 0)
                    {
                        SenseBlocking[a, b] = da.x != db.x;
                    }
                    else
                    {
                        SenseBlocking[a, b] = da.x != db.x && da.y != db.y;
                    }
                }
            }
        }

        public static bool IsViewBlocked(SenseDirection a, SenseDirection b)
        {
            return SenseBlocking[(int)a, (int)b];
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
            if (DirectionMapping.TryGetValue((Math.Sign(x), Math.Sign(y)), out var d))
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
            if (ReverseDirectionMapping.TryGetValue(d, out var result))
            {
                return result;
            }

            return (0, 0);
        }

        public override string ToString()
        {
            return $"{nameof(Direction)}: {Direction}, {nameof(Flags)}: {Flags}";
        }
    }

    public static class SenseDirectionExtensions
    {
        public static SenseDataFlags WithObstructed(this SenseDataFlags f, SenseDataFlags other)
        {
            return f | (other & SenseDataFlags.Obstructed);
        }

        /// <summary>
        ///   Marks all edges that are shared by both directions.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static SenseDirection Intersect(this SenseDirection a, SenseDirection b)
        {
            return a & b;
        }
    }
}