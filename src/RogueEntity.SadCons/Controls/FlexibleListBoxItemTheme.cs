using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Controls;
using SadConsole.Themes;

namespace RogueEntity.SadCons.Controls
{
    public class FlexibleListBoxItemTheme<T> : ThemeStates
    {
        readonly FlexibleCursor cursor;

        public FlexibleListBoxItemTheme()
        {
            cursor = new FlexibleCursor();
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
            cursor.AttachSurface(surface, area);
            if (Helpers.HasFlag(itemState, ControlStates.Selected) && !Helpers.HasFlag(itemState, ControlStates.MouseOver))
            {
                cursor.PrintAppearance = Selected;
            }
            else
            {
                cursor.PrintAppearance = GetStateAppearance(itemState);
            }

            cursor.Position = area.Location;
            try
            {
                DrawValue(cursor, area, item);
            }
            finally
            {
                cursor.DetachSurface();
            }
        }

        protected virtual ColoredString FormatValue(FlexibleCursor cursor, Rectangle area, T item)
        {
            string value = $"{item}";
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
        
        protected virtual void DrawValue(FlexibleCursor cursor, Rectangle area, T item)
        {
            cursor.Print(FormatValue(cursor, area, item));
        }

        public new virtual object Clone() => new FlexibleListBoxItemTheme<T>()
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
