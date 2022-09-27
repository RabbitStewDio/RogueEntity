using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using System;
using System.Runtime.CompilerServices;

namespace RogueEntity.Core.GridProcessing.Directionality
{
    public static class DirectionalityExtensions
    {
        static readonly DirectionalityInformation[] masks;

        static DirectionalityExtensions()
        {
            masks = new DirectionalityInformation[9];
            masks[(int)Direction.None] = (DirectionalityInformation) 0xFF; // Dummy mask to disallow any movement.
            masks[(int)Direction.Up] = DirectionalityInformation.Up;
            masks[(int)Direction.UpRight] = DirectionalityInformation.UpRight;
            masks[(int)Direction.Right] = DirectionalityInformation.Right;
            masks[(int)Direction.DownRight] = DirectionalityInformation.DownRight;
            masks[(int)Direction.Down] = DirectionalityInformation.Down;
            masks[(int)Direction.DownLeft] = DirectionalityInformation.DownLeft;
            masks[(int)Direction.Left] = DirectionalityInformation.Left;
            masks[(int)Direction.UpLeft] = DirectionalityInformation.UpLeft;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMovementAllowed(this DirectionalityInformation d, Direction dir)
        {
            var idx = (int)dir;
            var mask = masks[idx.Clamp(0, 9)];
            return (d & mask) == mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DirectionalityInformation With(this DirectionalityInformation d, Direction dir)
        {
            var idx = (int)dir;
            var mask = masks[idx.Clamp(0, 9)];
            return d | mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DirectionalityInformation WithOut(this DirectionalityInformation d, Direction dir)
        {
            var idx = (int)dir;
            var mask = masks[idx.Clamp(0, 9)];
            return d & ~mask;
        }

        public static string ToFormattedString(this DirectionalityInformation d)
        {
            var s = "";
            s += (d.IsMovementAllowed(Direction.Up)) ? "Up" : "__";
            s += (d.IsMovementAllowed(Direction.UpRight)) ? ",UR" : ",__";
            s += (d.IsMovementAllowed(Direction.Right)) ? ",Ri" : ",__";
            s += (d.IsMovementAllowed(Direction.DownRight)) ? ",DR" : ",__";
            s += (d.IsMovementAllowed(Direction.Down)) ? ",Dw" : ",__";
            s += (d.IsMovementAllowed(Direction.DownLeft)) ? ",DL" : ",__";
            s += (d.IsMovementAllowed(Direction.Left)) ? ",Le" : ",__";
            s += (d.IsMovementAllowed(Direction.UpLeft)) ? ",UL" : ",__";
            return s;
        }

        public static bool TryParse(string value, out DirectionalityInformation d)
        {
            var split = value.Split(new [] {','}, StringSplitOptions.None);
            d = DirectionalityInformation.None;
            
            foreach (var s in split)
            {
                d |= s switch
                {
                    "Up" => DirectionalityInformation.Up,
                    "UR" => DirectionalityInformation.UpRight,
                    "Ri" => DirectionalityInformation.Right,
                    "DR" => DirectionalityInformation.DownRight,
                    "Do" => DirectionalityInformation.Down,
                    "DL" => DirectionalityInformation.DownLeft,
                    "Le" => DirectionalityInformation.Left,
                    "UL" => DirectionalityInformation.UpLeft,
                    _ => DirectionalityInformation.None
                };
            }

            return true;
        }
    }
}