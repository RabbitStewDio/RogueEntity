using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Directionality
{
    public static class DirectionalityExtensions
    {
        static readonly DirectionalityInformation[] Masks;

        static DirectionalityExtensions()
        {
            Masks = new DirectionalityInformation[9];
            Masks[(int)Direction.None] = (DirectionalityInformation) 0xFF; // Dummy mask to disallow any movement.
            Masks[(int)Direction.Up] = DirectionalityInformation.Up;
            Masks[(int)Direction.UpRight] = DirectionalityInformation.UpRight;
            Masks[(int)Direction.Right] = DirectionalityInformation.Right;
            Masks[(int)Direction.DownRight] = DirectionalityInformation.DownRight;
            Masks[(int)Direction.Down] = DirectionalityInformation.Down;
            Masks[(int)Direction.DownLeft] = DirectionalityInformation.DownLeft;
            Masks[(int)Direction.Left] = DirectionalityInformation.Left;
            Masks[(int)Direction.UpLeft] = DirectionalityInformation.UpLeft;
        }
        
        public static bool IsMovementAllowed(this DirectionalityInformation d, Direction dir)
        {
            var idx = (int)dir;
            var mask = Masks[idx.Clamp(0, 9)];
            return (d & mask) == mask;
        }

        public static DirectionalityInformation With(this DirectionalityInformation d, Direction dir)
        {
            var idx = (int)dir;
            var mask = Masks[idx.Clamp(0, 9)];
            return d | mask;
        }

        public static DirectionalityInformation WithOut(this DirectionalityInformation d, Direction dir)
        {
            var idx = (int)dir;
            var mask = Masks[idx.Clamp(0, 9)];
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
    }
}