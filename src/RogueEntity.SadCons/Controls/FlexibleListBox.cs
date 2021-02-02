using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RogueEntity.Api.Utils;
using SadConsole;
using SadConsole.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace RogueEntity.SadCons.Controls
{
    [DataContract]
    public class FlexibleListBox<T> : ControlBase
    {
        static readonly EqualityComparer<T> EqualityComparer = EqualityComparer<T>.Default;

        public class SelectedItemEventArgs : EventArgs
        {
            public Optional<T> Item;

            public SelectedItemEventArgs(Optional<T> item) => Item = item;
        }

        protected bool initialized;

        Optional<T> selectedItem;

        [DataMember(Name = "SelectedIndex")]
        protected int selectedIndex;

        //[DataMember(Name = "BorderLines")]
        //protected int[] borderLineStyle;
        protected DateTime leftMouseLastClick = DateTime.Now;

        [DataMember(Name = "ScrollBarOffset")]
        protected Point scrollBarOffset = new Point(0, 0);

        [DataMember(Name = "ScrollBarSizeAdjust")]
        protected int scrollBarSizeAdjust = 0;

        [DataMember(Name = "ListItemHeight")]
        int listItemHeight;

        /// <summary>
        /// An event that triggers when the <see cref="SelectedItem"/> changes.
        /// </summary>
        public event EventHandler<SelectedItemEventArgs> SelectedItemChanged;

        /// <summary>
        /// An event that triggers when an item is double clicked or the Enter key is pressed while the listbox has focus.
        /// </summary>
        public event EventHandler<SelectedItemEventArgs> SelectedItemExecuted;

        /// <summary>
        /// The theme used by the listbox items.
        /// </summary>
        public FlexibleListBoxItemTheme<T> ItemTheme { get; private set; }

        /// <summary>
        /// Internal use only; used in rendering.
        /// </summary>
        public bool IsScrollBarVisible { get; set; }

        /// <summary>
        /// Used in rendering.
        /// </summary>
        [DataMember(Name = "ScrollBar")]
        public ScrollBar ScrollBar { get; private set; }

        /// <summary>
        /// Used in rendering.
        /// </summary>
        public Point ScrollBarRenderLocation { get; private set; }

        /// <summary>
        /// Used in rendering.
        /// </summary>
        public int HoveredListItemIndex { get; private set; }

        /// <summary>
        /// When the <see cref="SelectedItem"/> changes, and this property is true, objects will be compared by reference. If false, they will be compared by value.
        /// </summary>
        [DataMember]
        public bool CompareByReference { get; set; }

        /// <summary>
        /// When set to <see langword="true"/>, the <see cref="SelectedItemExecuted"/> event will fire when an item is single-clicked instead of double-clicked.
        /// </summary>
        [DataMember]
        public bool SingleClickItemExecute { get; set; }

        [IgnoreDataMember]
        public int ListItemHeight
        {
            get => listItemHeight;
            set
            {
                if (listItemHeight < 1) throw new ArgumentException();
                listItemHeight = value;
            }
        }

        [DataMember]
        public ObservableCollection<T> Items { get; private set; }

        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }
            set
            {
                if (value >= Items.Count || value < -1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, $"index {value} is invalid");
                }

                if (selectedIndex == value)
                {
                    return;
                }

                selectedIndex = value;
                if (selectedIndex == -1)
                {
                    selectedItem = Optional.Empty();
                }
                else
                {
                    selectedItem = Items[value];
                }
            }
        }

        public Optional<T> SelectedItem
        {
            get
            {
                if (SelectedIndex == -1)
                {
                    return Optional.Empty();
                }

                return Items[SelectedIndex];
            }
            set
            {
                int newIndex;
                if (value.TryGetValue(out var selectedValue))
                {
                    newIndex = Items.IndexOf(selectedValue);
                }
                else
                {
                    newIndex = -1;
                }

                this.selectedItem = value;
                if (newIndex != SelectedIndex)
                {
                    SelectedIndex = newIndex;
                    OnSelectedItemChanged();
                }
            }
        }

        public Point ScrollBarOffset
        {
            get => scrollBarOffset;
            set
            {
                scrollBarOffset = value;
                SetupScrollBar();
            }
        }

        public int ScrollBarSizeAdjust
        {
            get => scrollBarSizeAdjust;
            set
            {
                scrollBarSizeAdjust = value;
                SetupScrollBar();
            }
        }

        /// <summary>
        /// Creates a new instance of the listbox control.
        /// </summary>
        public FlexibleListBox(int width, int height) : base(width, height)
        {
            initialized = true;
            ScrollBarRenderLocation = new Point(width - 1, 0);
            SetupScrollBar();

            Items = new ObservableCollection<T>();
            Items.CollectionChanged += Items_CollectionChanged;

            ItemTheme = new FlexibleListBoxItemTheme<T>();
        }

        public FlexibleListBox(int width, int height, FlexibleListBoxItemTheme<T> itemTheme) : this(width, height) => ItemTheme = itemTheme;

        protected override void OnParentChanged() => ScrollBar.Parent = Parent;

        void _scrollbar_ValueChanged(object sender, EventArgs e) => IsDirty = true;

        protected virtual void OnSelectedItemChanged() => SelectedItemChanged?.Invoke(this, new SelectedItemEventArgs(SelectedItem));

        protected virtual void OnItemAction() => SelectedItemExecuted?.Invoke(this, new SelectedItemEventArgs(SelectedItem));

        protected override void OnPositionChanged() => ScrollBar.Position = Position + new Point(Width - 1, 0);

        protected void SetupScrollBar()
        {
            if (!initialized)
            {
                return;
            }

            //_scrollBar.Width, height < 3 ? 3 : height - _scrollBarSizeAdjust
            ScrollBar = new ScrollBar(Orientation.Vertical, Height);
            ScrollBar.ValueChanged += _scrollbar_ValueChanged;
            ScrollBar.IsVisible = false;
            ScrollBar.Position = Position + new Point(Width - 1, 0);
            OnThemeChanged();
            DetermineState();
        }

        protected override void OnThemeChanged()
        {
            if (ScrollBar == null) return;

            if (ActiveTheme is FlexibleListBoxTheme<T> theme)
            {
                ScrollBar.Theme = theme.ScrollBarTheme;
            }
            else
            {
                ScrollBar.Theme = null;
            }
        }

        void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            { }
            else if (e.Action == NotifyCollectionChangedAction.Move)
            { }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            { }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                ScrollBar.Value = 0;
            }

            if (selectedItem.TryGetValue(out var value))
            {
                selectedIndex = Items.IndexOf(value);
            }
            else
            {
                selectedItem = default;
                SelectedIndex = -1;
            }

            IsDirty = true;
        }

        public override bool ProcessKeyboard(SadConsole.Input.Keyboard info)
        {
            //if (_hasFocus)
            if (info.IsKeyReleased(Keys.Up))
            {
                var index = SelectedIndex;
                if (index > 0)
                {
                    SelectedIndex = index - 1;
                    if (index <= ScrollBar.Value)
                    {
                        ScrollBar.Value -= 1;
                    }
                }
                else
                {
                    SelectedIndex = Items.Count - 1;
                }

                return true;
            }

            if (info.IsKeyReleased(Keys.Down))
            {
                var index = SelectedIndex;
                if (index != Items.Count - 1)
                {
                    SelectedIndex = index + 1;
                    if (index + 1 >= ScrollBar.Value + (Height - 2))
                    {
                        ScrollBar.Value += 1;
                    }
                }

                return true;
            }

            if (info.IsKeyReleased(Keys.Enter))
            {
                if (selectedItem.HasValue)
                {
                    OnItemAction();
                }

                return true;
            }

            return false;
        }

        protected override void OnMouseIn(SadConsole.Input.MouseConsoleState state)
        {
            base.OnMouseIn(state);

            var rowOffset = ActiveFlexibleListBoxTheme.DrawBorder ? 1 : 0;
            var rowOffsetReverse = ActiveFlexibleListBoxTheme.DrawBorder ? 0 : 1;
            var columnOffsetEnd = IsScrollBarVisible || !ActiveFlexibleListBoxTheme.DrawBorder ? 1 : 0;

            var mouseControlPosition = new Point(state.CellPosition.X - Position.X, state.CellPosition.Y - Position.Y);

            if (mouseControlPosition.Y >= rowOffset && mouseControlPosition.Y < Height - rowOffset &&
                mouseControlPosition.X >= rowOffset && mouseControlPosition.X < Width - columnOffsetEnd)
            {
                // mouse is within the list ...
                
                var mouseRowRaw = (mouseControlPosition.Y - rowOffset) / ListItemHeight;
                if (IsScrollBarVisible)
                {
                    HoveredListItemIndex = mouseRowRaw + ScrollBar.Value;
                }
                else if (mouseControlPosition.Y <= Items.Count - rowOffsetReverse)
                {
                    HoveredListItemIndex = mouseRowRaw;
                }
                else
                {
                    HoveredListItemIndex = -1;
                }
            }
            else
            {
                HoveredListItemIndex = -1;
            }
        }

        FlexibleListBoxTheme<T> ActiveFlexibleListBoxTheme => (FlexibleListBoxTheme<T>)ActiveTheme;

        protected override void OnLeftMouseClicked(SadConsole.Input.MouseConsoleState state)
        {
            base.OnLeftMouseClicked(state);

            var click = DateTime.Now;
            var doubleClicked = (click - leftMouseLastClick).TotalSeconds <= 0.5;
            leftMouseLastClick = click;

            var rowOffset = ActiveFlexibleListBoxTheme.DrawBorder ? 1 : 0;
            var rowOffsetReverse = ActiveFlexibleListBoxTheme.DrawBorder ? 0 : 1;
            var columnOffsetEnd = IsScrollBarVisible || !ActiveFlexibleListBoxTheme.DrawBorder ? 1 : 0;

            var mouseControlPosition = new Point(state.CellPosition.X - Position.X, state.CellPosition.Y - Position.Y);

            if (mouseControlPosition.Y >= rowOffset && mouseControlPosition.Y < Height - rowOffset &&
                mouseControlPosition.X >= rowOffset && mouseControlPosition.X < Width - columnOffsetEnd)
            {
                Optional<T> oldItem = selectedItem;
                var noItem = false;

                if (IsScrollBarVisible)
                {
                    selectedIndex = mouseControlPosition.Y - rowOffset + ScrollBar.Value;
                    SelectedItem = Items[selectedIndex];
                }
                else if (mouseControlPosition.Y <= Items.Count - rowOffsetReverse)
                {
                    selectedIndex = mouseControlPosition.Y - rowOffset;
                    SelectedItem = Items[selectedIndex];
                }
                else
                {
                    noItem = true;
                }

                if (!noItem && (SingleClickItemExecute || (doubleClicked && oldItem == SelectedItem)))
                {
                    leftMouseLastClick = DateTime.MinValue;
                    OnItemAction();
                }
            }
        }

        /// <inheritdoc />
        public override bool ProcessMouse(SadConsole.Input.MouseConsoleState state)
        {
            if (isEnabled)
            {
                if (isMouseOver)
                {
                    var mouseControlPosition = TransformConsolePositionByControlPosition(state.CellPosition);

                    if (mouseControlPosition.X == ScrollBarRenderLocation.X && IsScrollBarVisible)
                    {
                        ScrollBar.ProcessMouse(state);
                    }
                    else
                    {
                        if (IsScrollBarVisible && state.Mouse.ScrollWheelValueChange != 0)
                        {
                            ScrollBar.Value += state.Mouse.ScrollWheelValueChange / 20;
                            return true;
                        }

                        base.ProcessMouse(state);
                    }
                }
                else
                {
                    base.ProcessMouse(state);
                }
            }

            return false;
        }

        [OnDeserialized]
        void AfterDeserialized(StreamingContext context)
        {
            initialized = true;

            ScrollBar.ValueChanged += _scrollbar_ValueChanged;
            Items.CollectionChanged += Items_CollectionChanged;

            SetupScrollBar();

            DetermineState();
            IsDirty = true;
        }
    }
}
