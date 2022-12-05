using System;

namespace RogueEntity.Core.Utils.SpatialIndex
{
    public readonly struct AABB
    {
        public readonly int CenterX;
        public readonly int CenterY;
        public readonly int ExtendX;
        public readonly int ExtendY;

        public AABB(int centerX, int centerY, int extendX, int extendY)
        {
            CenterX = centerX;
            CenterY = centerY;
            ExtendX = extendX;
            ExtendY = extendY;
        }

        public static implicit operator AABB(BoundingBox b)
        {
            var centerX = (b.Left + b.Right) / 2;
            var centerY = (b.Top + b.Bottom) / 2;
            var extendX = Math.Abs(b.Right - b.Left) / 2;
            var extendY = Math.Abs(b.Bottom - b.Top) / 2;
            return new AABB(centerX, centerY, extendX, extendY);
        }

        public static implicit operator BoundingBox(AABB b)
        {
            return BoundingBox.From(b.CenterX - b.ExtendX, b.CenterY - b.ExtendY,
                                    b.CenterX + b.ExtendX, b.CenterY + b.ExtendY);
        }

        public void Deconstruct(out int centerX, out int centerY, out int extendX, out int extendY)
        {
            centerX = CenterX;
            centerY = CenterY;
            extendX = ExtendX;
            extendY = ExtendY;
        }

        public override string ToString()
        {
            return $"AABB({nameof(CenterX)}: {CenterX}, {nameof(CenterY)}: {CenterY}, {nameof(ExtendX)}: {ExtendX}, {nameof(ExtendY)}: {ExtendY})";
        }
    }
}