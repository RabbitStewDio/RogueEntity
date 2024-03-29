﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SadConsole;
using System;
using System.Runtime.Serialization;
using SadConsole.Controls;
using SadConsole.Themes;
using SadConsole.Effects;

namespace RogueEntity.SadCons.Controls
{
    /// <summary>
    /// A theme for the input box control.
    /// </summary>
    [DataContract]
    public class FlexibleTextBoxTheme : ThemeBase
    {
        int oldCaretPosition;
        ControlStates oldState;
        string editingText;

        /// <summary>
        /// The style to use for the carrot.
        /// </summary>
        [DataMember]
        public ICellEffect CaretEffect;

        /// <summary>
        /// Creates a new theme used by the <see cref="TextBox"/>.
        /// </summary>
        public FlexibleTextBoxTheme() => CaretEffect = new BlinkGlyph()
        {
            GlyphIndex = 95,
            BlinkSpeed = 0.4f
        };

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
        public override void RefreshTheme(Colors themeColors, ControlBase control)
        {
            themeColors ??= Library.Default.Colors;

            base.RefreshTheme(themeColors, control);

            Normal = new Cell(themeColors.Text, themeColors.GrayDark);
        }

        /// <inheritdoc />
        public override void UpdateAndDraw(ControlBase control, TimeSpan time)
        {
            if (!(control is FlexibleTextBox textbox))
            {
                return;
            }

            if (textbox.Surface.Effects.Count != 0)
            {
                textbox.Surface.Effects.UpdateEffects(time.TotalSeconds);
                textbox.IsDirty = true;
            }

            if (!textbox.IsDirty)
            {
                return;
            }

            RefreshTheme(control.ThemeColors, control);
            Cell appearance = GetStateAppearance(textbox.State);

            if (textbox.IsFocused && !textbox.DisableKeyboard)
            {
                if (!textbox.IsCaretVisible)
                {
                    oldCaretPosition = textbox.CaretPosition;
                    oldState = textbox.State;
                    editingText = textbox.EditingText;
                    textbox.Surface.Fill(appearance.Foreground, appearance.Background, 0, SpriteEffects.None);
                    if (string.IsNullOrEmpty(textbox.PasswordChar))
                    {
                        textbox.Surface.Print(0, 0, textbox.EditingText.Substring(textbox.LeftDrawOffset));
                    }
                    else
                    {
                        textbox.Surface.Print(0, 0, textbox.EditingText.Substring(textbox.LeftDrawOffset).Masked(textbox.PasswordChar));
                    }

                    textbox.Surface.SetEffect(textbox.Surface[textbox.CaretPosition - textbox.LeftDrawOffset, 0], CaretEffect);
                    textbox.IsCaretVisible = true;
                }

                else if (oldCaretPosition != textbox.CaretPosition || oldState != textbox.State || editingText != textbox.EditingText)
                {
                    textbox.Surface.Effects.RemoveAll();
                    textbox.Surface.Fill(appearance.Foreground, appearance.Background, 0, SpriteEffects.None);
                    if (string.IsNullOrEmpty(textbox.PasswordChar))
                    {
                        textbox.Surface.Print(0, 0, textbox.EditingText.Substring(textbox.LeftDrawOffset));
                    }
                    else
                    {
                        textbox.Surface.Print(0, 0, textbox.EditingText.Substring(textbox.LeftDrawOffset).Masked(textbox.PasswordChar));
                    }

                    // TODO: If the keyboard repeat is down and the text goes off the end of the textbox and we're hitting the left arrow then sometimes control.LeftDrawOffset can exceed control.CaretPosition
                    // This causes an Out of Bounds error here.  I don't think it's new - I think it's been in for a long time so I'm gonna check in and come back to this.
                    // It might be that we just need to take Max(0, "bad value") below but I think it should be checked into to really understand the situation.
                    textbox.Surface.SetEffect(control.Surface[textbox.CaretPosition - textbox.LeftDrawOffset, 0], CaretEffect);
                    oldCaretPosition = textbox.CaretPosition;
                    oldState = control.State;
                    editingText = textbox.EditingText;
                }
            }
            else
            {
                textbox.Surface.Effects.RemoveAll();
                textbox.Surface.Fill(appearance.Foreground, appearance.Background, appearance.Glyph, appearance.Mirror);
                textbox.IsCaretVisible = false;
                if (string.IsNullOrEmpty(textbox.PasswordChar))
                {
                    textbox.Surface.Print(0, 0, textbox.Text.Align(textbox.TextAlignment, textbox.Width));
                }
                else
                {
                    textbox.Surface.Print(0, 0, textbox.Text.Masked(textbox.PasswordChar).Align(textbox.TextAlignment, textbox.Width));
                }
            }

            textbox.IsDirty = false;
        }


        /// <inheritdoc />
        public override ThemeBase Clone() => new FlexibleTextBoxTheme()
        {
            Normal = Normal.Clone(),
            Disabled = Disabled.Clone(),
            MouseOver = MouseOver.Clone(),
            MouseDown = MouseDown.Clone(),
            Selected = Selected.Clone(),
            Focused = Focused.Clone(),
            CaretEffect = CaretEffect.Clone()
        };
    }
}
