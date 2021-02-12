using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Controls;
using SadConsole.Themes;
using System;

namespace RogueEntity.SadCons.Controls
{
    public class FlexibleListBoxItemTheme<T> : ThemeStates
    {
        protected readonly FlexibleCursor Cursor;
        public int ItemHeight { get; }

        public FlexibleListBoxItemTheme(int itemHeight = 1)
        {
            if (itemHeight < 1)
            {
                throw new ArgumentException();
            }

            ItemHeight = itemHeight;
            Cursor = new FlexibleCursor();
        }

        /// <inheritdoc />
        public override void RefreshTheme(Colors themeColors, ControlBase control)
        {
            if (themeColors == null) themeColors = Library.Default.Colors;

            base.RefreshTheme(themeColors, control);

            SetForeground(Normal.Foreground);
            SetBackground(Normal.Background);

            Selected.Foreground = themeColors.Appearance_ControlSelected.Foreground;
            MouseOver = themeColors.Appearance_ControlOver.Clone();
        }

        public virtual void Draw(CellSurface surface, Rectangle area, T item, ControlStates itemState)
        {
            try
            {
                Cursor.AttachSurface(surface, area);
                Cursor.AutomaticallyShiftRowsUp = false;
                Cursor.Position = new Point(0, 0);
                
                if (Helpers.HasFlag(itemState, ControlStates.Selected) && !Helpers.HasFlag(itemState, ControlStates.MouseOver))
                {
                    Cursor.SetPrintAppearance(Selected);
                }
                else
                {
                    Cursor.SetPrintAppearance(GetStateAppearance(itemState));
                }

                surface.Fill(area, Cursor.PrintAppearance.Foreground, Cursor.PrintAppearance.Background, 0);

                DrawValue(Cursor, area, item, itemState);
            }
            finally
            {
                Cursor.DetachSurface();
            }
        }

        protected virtual ColoredString FormatValue(FlexibleCursor cursor, Rectangle area, T item, ControlStates state)
        {
            string value = $"{item}";
            return FormatValueFromString(cursor, area, value);
        }

        protected ColoredString FormatValueFromString(FlexibleCursor cursor, Rectangle area, string value)
        {
            if (value.Length < area.Width)
            {
                value += new string(' ', area.Width - value.Length);
            }
            else if (value.Length > area.Width)
            {
                value = value.Substring(0, area.Width);
            }

            return cursor.PrepareColoredString(value);
        }

        protected virtual void DrawValue(FlexibleCursor cursor, Rectangle area, T item, ControlStates state)
        {
            var value = FormatValue(cursor, area, item, state);
            cursor.Print(value);
        }

        public new virtual object Clone() => new FlexibleListBoxItemTheme<T>(ItemHeight)
        {
            Normal = Normal.Clone(),
            Disabled = Disabled.Clone(),
            MouseOver = MouseOver.Clone(),
            MouseDown = MouseDown.Clone(),
            Selected = Selected.Clone(),
            Focused = Focused.Clone(),
        };
    }
}
