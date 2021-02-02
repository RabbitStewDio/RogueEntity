using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Controls;
using SadConsole.Themes;
using System;
using System.Runtime.Serialization;

namespace RogueEntity.SadCons.Controls
{
    /// <summary>
    /// The theme for a ListBox control.
    /// </summary>
    [DataContract]
    public class FlexibleListBoxTheme<T> : ThemeBase
    {
        /// <summary>
        /// The drawing theme for the boarder when <see cref="DrawBorder"/> is true.
        /// </summary>
        [DataMember]
        public ThemeStates BorderTheme;

        /// <summary>
        /// The line style for the border when <see cref="DrawBorder"/> is true.
        /// </summary>
        [DataMember]
        public int[] BorderLineStyle;

        /// <summary>
        /// If false the border will not be drawn.
        /// </summary>
        [DataMember]
        public bool DrawBorder;

        /// <summary>
        /// The appearance of the scrollbar used by the listbox control.
        /// </summary>
        [DataMember]
        public ScrollBarTheme ScrollBarTheme;

        /// <summary>
        /// Creates a new theme used by the <see cref="ListBox"/>.
        /// </summary>
        /// <param name="scrollBarTheme">The theme to use to draw the scroll bar.</param>
        public FlexibleListBoxTheme(ScrollBarTheme scrollBarTheme)
        {
            ScrollBarTheme = scrollBarTheme;
            BorderTheme = new ThemeStates();
        }

        /// <inheritdoc />
        public override void Attached(ControlBase control)
        {
            control.Surface = new CellSurface(control.Width, control.Height)
            {
                DefaultBackground = Color.Transparent
            };
            control.Surface.Clear();

            base.Attached(control);
        }

        /// <inheritdoc />
        public override void UpdateAndDraw(ControlBase control, TimeSpan time)
        {
            if (!(control is FlexibleListBox<T> listbox))
            {
                return;
            }

            if (!listbox.IsDirty)
            {
                return;
            }

            RefreshTheme(control.ThemeColors, control);
            
            int columnOffset;
            int columnEnd;
            int startingRow;
            int endingRow;

            Cell appearance = GetStateAppearance(listbox.State);
            Cell borderAppearance = BorderTheme.GetStateAppearance(listbox.State);

            // Redraw the control
            listbox.Surface.Fill(
                appearance.Foreground,
                appearance.Background,
                appearance.Glyph);

            if (DrawBorder)
            {
                endingRow = listbox.Height - 2;
                startingRow = 1;
                columnOffset = 1;
                columnEnd = listbox.Width - 2;
                listbox.Surface.DrawBox(new Rectangle(0, 0, listbox.Width, listbox.Height), new Cell(borderAppearance.Foreground, borderAppearance.Background, 0), null, BorderLineStyle);
            }
            else
            {
                endingRow = listbox.Height;
                startingRow = 0;
                columnOffset = 0;
                columnEnd = listbox.Width;
                listbox.Surface.Fill(borderAppearance.Foreground, borderAppearance.Background, 0);
            }

            ShowHideScrollBar(listbox);

            int itemOffset = listbox.IsScrollBarVisible ? listbox.ScrollBar.Value : 0;
            for (int renderedItem = itemOffset; renderedItem < listbox.Items.Count; renderedItem += 1)
            {
                var relativeItem = renderedItem - itemOffset;
                if (relativeItem * listbox.ListItemHeight > endingRow)
                {
                    return;
                }
                
                var state = ControlStates.Normal;
                if (Helpers.HasFlag(listbox.State, ControlStates.MouseOver) && listbox.HoveredListItemIndex == renderedItem)
                {
                    Helpers.SetFlag(ref state, ControlStates.MouseOver);
                }

                if (Helpers.HasFlag(listbox.State, ControlStates.MouseLeftButtonDown))
                {
                    Helpers.SetFlag(ref state, ControlStates.MouseLeftButtonDown);
                }

                if (Helpers.HasFlag(listbox.State, ControlStates.MouseRightButtonDown))
                {
                    Helpers.SetFlag(ref state, ControlStates.MouseRightButtonDown);
                }

                if (Helpers.HasFlag(listbox.State, ControlStates.Disabled))
                {
                    Helpers.SetFlag(ref state, ControlStates.Disabled);
                }

                if (renderedItem == listbox.SelectedIndex)
                {
                    Helpers.SetFlag(ref state, ControlStates.Selected);
                }

                var renderAreaY = startingRow + relativeItem * listbox.ListItemHeight;
                var renderAreaY2 = Math.Min(renderAreaY + listbox.ListItemHeight, endingRow);
                var bounds = new Rectangle(columnOffset, renderAreaY, columnEnd, renderAreaY2 - renderAreaY);
                
                listbox.ItemTheme.Draw(listbox.Surface, bounds, listbox.Items[renderedItem], state);
            }

            if (listbox.IsScrollBarVisible)
            {
                listbox.ScrollBar.IsDirty = true;
                listbox.ScrollBar.Update(time);
                int y = listbox.ScrollBarRenderLocation.Y;

                for (int yCell = 0; yCell < listbox.ScrollBar.Height; yCell++)
                {
                    listbox.Surface.SetGlyph(listbox.ScrollBarRenderLocation.X, y, listbox.ScrollBar.Surface[0, yCell].Glyph);
                    listbox.Surface.SetCellAppearance(listbox.ScrollBarRenderLocation.X, y, listbox.ScrollBar.Surface[0, yCell]);
                    y++;
                }
            }


            listbox.IsDirty = Helpers.HasFlag(listbox.State, ControlStates.MouseOver);
        }

        public override void RefreshTheme(Colors colors, ControlBase control)
        {
            if (colors == null) colors = Library.Default.Colors;

            var listbox = (FlexibleListBox<T>)control;

            base.RefreshTheme(colors, control);

            SetForeground(Normal.Foreground);
            SetBackground(Normal.Background);
            listbox.ItemTheme.RefreshTheme(colors, control);

            listbox.ScrollBar.Theme = ScrollBarTheme;

            ScrollBarTheme?.RefreshTheme(colors, listbox.ScrollBar);

            BorderTheme.RefreshTheme(colors, control);
            BorderTheme.SetForeground(Normal.Foreground);
            BorderTheme.SetBackground(Normal.Background);
            BorderLineStyle = (int[])CellSurface.ConnectedLineThick.Clone();
        }

        /// <inheritdoc />
        public override ThemeBase Clone() => new FlexibleListBoxTheme<T>((ScrollBarTheme)ScrollBarTheme.Clone())
        {
            Normal = Normal.Clone(),
            Disabled = Disabled.Clone(),
            MouseOver = MouseOver.Clone(),
            MouseDown = MouseDown.Clone(),
            Selected = Selected.Clone(),
            Focused = Focused.Clone(),
            BorderTheme = BorderTheme?.Clone(),
            BorderLineStyle = (int[])BorderLineStyle?.Clone(),
            DrawBorder = DrawBorder,
        };

        public void ShowHideScrollBar(FlexibleListBox<T> control)
        {
            int heightOffset = DrawBorder ? 2 : 0;

            // process the scroll bar
            int scrollbarItems = control.Items.Count - (control.Height - heightOffset);

            if (scrollbarItems > 0)
            {
                control.ScrollBar.Maximum = scrollbarItems;
                control.IsScrollBarVisible = true;
            }
            else
            {
                control.ScrollBar.Maximum = 0;
                control.IsScrollBarVisible = false;
            }
        }
    }
}
