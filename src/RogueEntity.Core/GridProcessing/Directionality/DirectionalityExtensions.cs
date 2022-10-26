using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Runtime.CompilerServices;
using System.Text;

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

        public static string PrintEdges(this IReadOnlyDynamicDataView2D<DirectionalityInformation> map) => PrintEdges(map, map.GetActiveBounds());

        public static string PrintEdges(this IReadOnlyView2D<DirectionalityInformation> map, Rectangle bounds)
        {
            var yMin = bounds.Y;
            var yMax = bounds.MaxExtentY;
            var xMin = bounds.X;
            var xMax = bounds.MaxExtentX;
            StringBuilder sb = new StringBuilder();

            for (var y = yMin; y <= yMax; y += 1)
            {
                for (var x = xMin; x <= xMax; x += 1)
                {
                    map.TryGet(x, y, out var data);
                    if ((data & DirectionalityInformation.UpLeft) != 0)
                    {
                        sb.Append("\\");
                    }
                    else
                    {
                        sb.Append(".");
                    }

                    if ((data & DirectionalityInformation.Up) != 0)
                    {
                        sb.Append("|");
                    }
                    else
                    {
                        sb.Append(".");
                    }

                    if ((data & DirectionalityInformation.UpRight) != 0)
                    {
                        sb.Append("/");
                    }
                    else
                    {
                        sb.Append(".");
                    }

                    sb.Append(" ");
                }

                sb.AppendLine();

                for (var x = xMin; x <= xMax; x += 1)
                {
                    map.TryGet(x, y, out var data);
                    if ((data & DirectionalityInformation.Left) != 0)
                    {
                        sb.Append("-");
                    }
                    else
                    {
                        sb.Append(".");
                    }

                    sb.Append("*");

                    if ((data & DirectionalityInformation.Right) != 0)
                    {
                        sb.Append("-");
                    }
                    else
                    {
                        sb.Append(".");
                    }

                    sb.Append(" ");
                }

                sb.AppendLine();

                for (var x = xMin; x <= xMax; x += 1)
                {
                    map.TryGet(x, y, out var data);
                    if ((data & DirectionalityInformation.DownLeft) != 0)
                    {
                        sb.Append("/");
                    }
                    else
                    {
                        sb.Append(".");
                    }

                    if ((data & DirectionalityInformation.Down) != 0)
                    {
                        sb.Append("|");
                    }
                    else
                    {
                        sb.Append(".");
                    }

                    if ((data & DirectionalityInformation.DownRight) != 0)
                    {
                        sb.Append("\\");
                    }
                    else
                    {
                        sb.Append(".");
                    }

                    sb.Append(" ");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }        
    }
}