using Microsoft.Xna.Framework;
using RogueEntity.SadCons.Controls;
using SadConsole.Controls;
using System;
using System.Globalization;

namespace RogueEntity.SadCons
{
    public static class SadConsoleControls
    {
        public static Button CreateButton(string text, int width, int height)
        {
            return new Button(width, height)
            {
                Text = text
            };
        }

        public static RadioButton CreateRadioButton<T>(string text, RadioButtonSet<T> group, T value, int width, int height = 1)
        {
            var radioButton = new RadioButton(width, height)
            {
                Text = text,
                GroupName = @group.Name
            };
            group.Register(value, radioButton);
            return radioButton;
        }

        public static Label CreateLabel(string text)
        {
            return new Label(text);
        }

        public static Label CreateLabel(string text, int width)
        {
            return new Label(width)
            {
                DisplayText = text
            };
        }

        public static Label WithBinding(this Label label, ILabelBinding binding)
        {
            binding.PropertyChanged += (s, e) =>
            {
                label.DisplayText = binding.FormattedValue;
            };
            return label;
        }

        public static FlexibleTextBox CreateTextBox(string text, int width)
        {
            return new FlexibleTextBox(width)
            {
                Text = text
            };
        }

        public static FlexibleTextBox CreateDecimalTextBox(string text, int width, NumberStyles style = NumberStyles.Integer)
        {
            return new FlexibleTextBox(width)
            {
                IsNumeric = true,
                AllowDecimal = style == NumberStyles.Number || style == NumberStyles.Float,
                Text = text,
            };
        }

        public static FlexibleTextBox WithInputHandler(this FlexibleTextBox t, Action<FlexibleTextBox> act)
        {
            t.TextChanged += (s, e) => act(t);
            return t;
        }

        public static TControl With<TControl>(this TControl t, Action<TControl> act)
        {
            act(t);
            return t;
        }

        public static TControl WithPlacementAt<TControl>(this TControl t, int x, int y)
            where TControl : ControlBase
        {
            t.Position = new Point(x, y);
            return t;
        }

        public static TControl WithVerticalPlacementAt<TControl>(this TControl t, int x, ref int y, int padding = 1)
            where TControl : ControlBase
        {
            t.Position = new Point(x, y);
            y += t.Height;
            y += padding;
            return t;
        }

        public static TControl WithHorizontalPlacementAt<TControl>(this TControl t, ref int x, int y, int padding = 2)
            where TControl : ControlBase
        {
            t.Position = new Point(x, y);
            x += t.Width;
            x += padding;
            return t;
        }

        public static TControl WithAction<TControl>(this TControl t, Action act)
            where TControl : ButtonBase
        {
            t.Click += (o, e) => act();
            return t;
        }

        public static RadioButton WithAction(this RadioButton t, Action act)
        {
            t.IsSelectedChanged += (o, e) => act();
            return t;
        }

        public static RadioButton WithSelection<T>(this RadioButton b, RadioButtonSet<T> t, T selector)
        {
            t.Register(selector, b);
            return b;
        }

        public static FlexibleTextBox WithText(this FlexibleTextBox t, string text)
        {
            if (t != null)
            {
                t.Text = text;
            }

            return t;
        }
    }
}
