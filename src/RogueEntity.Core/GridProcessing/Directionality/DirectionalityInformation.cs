using System;

namespace RogueEntity.Core.GridProcessing.Directionality
{
    [Flags]
    public enum DirectionalityInformation : byte
    {
        None = 0,
        Up = 1,
        UpRight = 2,
        Right = 4,
        DownRight = 8,
        Down = 16,
        DownLeft = 32,
        Left = 64,
        UpLeft = 128,
        All = 255
    }
}