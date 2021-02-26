using Microsoft.Xna.Framework;
using SadConsole;
using System;

namespace RogueEntity.SadCons
{
    public static class SadConsoleExtensions
    {
        public static Cell GetCellAt(this CellSurface s, int x, int y)
        {
            if (!s.IsValidCell(x, y, out int index))
            {
                throw new IndexOutOfRangeException();
            }

            return s.Cells[index];
        }

        public static Cell MergeAppearanceFrom(this Cell c, Cell template)
        {
            if (c.Glyph == 0)
            {
                c.Glyph = template.Glyph;
            }

            if (c.Background == Color.Transparent)
            {
                c.Background = template.Background;
            }

            if (c.Foreground == Color.Transparent)
            {
                c.Foreground = template.Foreground;
            }

            return c;
        }
    }
}
