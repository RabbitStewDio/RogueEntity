using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Sensing.Common
{
    public readonly struct SenseDirectionStore
    {
        static readonly Dictionary<(int, int), SenseDirection> DirectionMapping;
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
        }
        public readonly byte RawData;

        public SenseDirectionStore(byte rawData)
        {
            this.RawData = rawData;
        }

        public SenseDirection Direction => (SenseDirection) (RawData & 0xf);
        public SenseDataFlags Flags => (SenseDataFlags) ((RawData >> 4) & 0xf);

        public static SenseDirectionStore From(SenseDirection dir, SenseDataFlags flags)
        {
            var raw = (int)dir | ((int)flags << 4) ;
            return new SenseDirectionStore((byte) raw);
        }
        
        public static SenseDirectionStore From(int x, int y)
        {
            if (DirectionMapping.TryGetValue((Math.Sign(x), Math.Sign(y)), out var d))
            {
                return new SenseDirectionStore((byte) d);
            }
            
            return new SenseDirectionStore(0);
        }

        public SenseDirectionStore With(SenseDataFlags f)
        {
            var direction = (RawData & DirectionMask);
            var flags = (int)f << 4;
            return new SenseDirectionStore((byte)(direction | flags));
        }
        
    }
}