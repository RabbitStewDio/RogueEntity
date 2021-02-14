using Microsoft.Xna.Framework.Input;
using SadConsole;
using SadConsole.Controls;
using SadConsole.Input;
using SadConsole.Themes;
using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

namespace RogueEntity.SadCons.Controls
{
    public class FlexibleTextBox : ControlBase
    {
        static FlexibleTextBox()
        {
            Library.Default.SetControlTheme(typeof(FlexibleTextBox), new FlexibleTextBoxTheme());
        }
        
        string editingText;
        bool disableKeyboardEdit;

        /// <summary>
        /// Mask input with a certain character.
        /// </summary>
        public string PasswordChar;

        /// <summary>
        /// Indicates the caret is visible.
        /// </summary>
        public bool IsCaretVisible = false;

        /// <summary>
        /// The alignment of the text.
        /// </summary>
        [DataMember(Name = "TextAlignment")]
        protected HorizontalAlignment Alignment = HorizontalAlignment.Left;

        /// <summary>
        /// When editing the text box, this allows the text to scroll to the right so you can see what you are typing.
        /// </summary>
        public int LeftDrawOffset { get; protected set; }

        /// <summary>
        /// The location of the caret.
        /// </summary>
        [DataMember(Name = "CaretPosition")]
        int caretPos;

        /// <summary>
        /// The text value of the input box.
        /// </summary>
        [DataMember(Name = "Text")]
        string text = "";

        /// <summary>
        /// Indicates the input box is numeric only.
        /// </summary>
        [DataMember(Name = "IsNumeric")]
        bool isNumeric;

        /// <summary>
        /// Indicates that the input box (when numeric) will accept decimal points.
        /// </summary>
        [DataMember(Name = "AllowDecimalPoint")]
        bool allowDecimalPoint;

        /// <summary>
        /// The current appearance of the control.
        /// </summary>
        Cell currentAppearance;

        /// <summary>
        /// Raised when the text has changed and the preview has accepted it.
        /// </summary>
        public event EventHandler TextChanged;

        /// <summary>
        /// Raised before the text has changed and allows the change to be cancelled.
        /// </summary>
        public event EventHandler<TextChangedEventArgs> TextChangedPreview;

        /// <summary>
        /// Raised when a key is pressed on the textbox.
        /// </summary>
        public event EventHandler<KeyPressEventArgs> KeyPressed;

        /// <summary>
        /// Disables mouse input.
        /// </summary>
        [DataMember(Name = "DisableMouseInput")]
        public bool DisableMouse;

        /// <summary>
        /// Disables the keyboard which turns off keyboard input and hides the cursor.
        /// </summary>
        [DataMember(Name = "DisableKeyboardInput")]
        public bool DisableKeyboard
        {
            get => disableKeyboardEdit;
            set
            {
                disableKeyboardEdit = value;

                if (!disableKeyboardEdit)
                {
                    caretPos = Text.Length;
                }
            }
        }

        /// <summary>
        /// A temp holder for the text as it's being edited.
        /// </summary>
        public string EditingText
        {
            get => editingText;
            protected set
            {
                editingText = value;

                if (MaxLength != 0)
                {
                    if (editingText.Length >= MaxLength)
                    {
                        editingText = editingText.Substring(0, MaxLength);
                    }
                }

                ValidateCursorPosition();
                DetermineState();
                IsDirty = true;
            }
        }

        /// <summary>
        /// The alignment of the caret.
        /// </summary>
        public HorizontalAlignment TextAlignment
        {
            get => Alignment;
            set
            {
                Alignment = value;
                DetermineState();
                IsDirty = true;
            }
        }

        /// <summary>
        /// How big the text can be. Setting this to 0 will make it unlimited.
        /// </summary>
        [DataMember]
        public int MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the position of the caret in the current text.
        /// </summary>
        public int CaretPosition
        {
            get => caretPos;
            set
            {
                caretPos = value;
                DetermineState();
                IsDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the text of the input box.
        /// </summary>
        public string Text
        {
            get => text;
            set
            {
                UpdateText(value);
                TextChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void UpdateText(string value)
        {
            if (value == text)
            {
                return;
            }

            var trimmedValue = MaxLength != 0 && value.Length > MaxLength ? value.Substring(0, MaxLength) : value;
            var args = new TextChangedEventArgs(text, trimmedValue);

            TextChangedPreview?.Invoke(this, args);

            text = args.NewValue ?? "";
            text = MaxLength != 0 && text.Length > MaxLength ? text.Substring(0, MaxLength) : text;

            Validate();
            EditingText = text;
            caretPos = Text.Length;
        }
        
        /// <summary>
        /// Gets or sets weather or not this input box only allows numeric input.
        /// </summary>
        public bool IsNumeric
        {
            get => isNumeric;
            set
            {
                isNumeric = value;
                Validate();
            }
        }

        /// <summary>
        /// Gets or sets weather or not this input box should restrict numeric input should allow a decimal point.
        /// </summary>
        public bool AllowDecimal
        {
            get => allowDecimalPoint;
            set
            {
                allowDecimalPoint = value;
                Validate();
            }
        }

        public Func<FlexibleTextBox, string, string> Formatter
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new instance of the input box.
        /// </summary>
        /// <param name="width">The width of the input box.</param>
        public FlexibleTextBox(int width)
            : base(width, 1)
        { }


        /// <summary>
        /// Validates that the value of the input box conforms to the settings of this control and sets the dirty flag to true.
        /// </summary>
        protected void Validate()
        {
            if (Formatter != null)
            {
                text = Formatter(this, text);
            }
            else
            {
                text = DefaultFormatter(this, text);
            }

            DetermineState();
            IsDirty = true;
        }

        static string DefaultFormatter(FlexibleTextBox tx, string text)
        {
            if (string.IsNullOrEmpty(text) || !tx.IsNumeric)
            {
                return text;
            }

            if (tx.AllowDecimal)
            {
                if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out var value))
                {
                    value = 0;
                }
                return $"{value:N}";
            }
            else
            {
                if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var value))
                {
                    value = 0;
                }
                return $"{value:D}";
            }
        }

        /// <summary>
        /// Correctly positions the cursor within the text.
        /// </summary>
        protected void ValidateCursorPosition()
        {
            if (MaxLength != 0)
            {
                if (caretPos > EditingText.Length)
                {
                    caretPos = EditingText.Length - 1;
                }
            }
            else if (caretPos > EditingText.Length)
            {
                caretPos = EditingText.Length;
            }


            // Test to see if caret is off edge of box
            if (caretPos >= Width)
            {
                LeftDrawOffset = EditingText.Length - Width + 1;

                if (LeftDrawOffset < 0)
                {
                    LeftDrawOffset = 0;
                }
            }
            else
            {
                LeftDrawOffset = 0;
            }

            DetermineState();
            IsDirty = true;
        }


        bool TriggerKeyPressEvent(AsciiKey key)
        {
            if (KeyPressed == null)
            {
                return false;
            }

            var args = new KeyPressEventArgs(key);
            KeyPressed(this, args);

            return args.IsCancelled;
        }


        /// <summary>
        /// Called when the control should process keyboard information.
        /// </summary>
        /// <param name="info">The keyboard information.</param>
        /// <returns>True if the keyboard was handled by this control.</returns>
        public override bool ProcessKeyboard(SadConsole.Input.Keyboard info)
        {
            if (info.KeysPressed.Count == 0)
            {
                return false;
            }

            if (DisableKeyboard)
            {
                for (int i = 0; i < info.KeysPressed.Count; i++)
                {
                    if (info.KeysPressed[i].Key == Keys.Enter)
                    {
                        if (TriggerKeyPressEvent(info.KeysPressed[i]))
                        {
                            return false;
                        }

                        IsDirty = true;
                        DisableKeyboard = false;
                        Text = EditingText;
                    }
                }

                return true;
            }

            var newText = new StringBuilder(EditingText, Width - 1);
            IsDirty = true;
            for (int i = 0; i < info.KeysPressed.Count; i++)
            {
                var currentKey = info.KeysPressed[i];
                if (TriggerKeyPressEvent(currentKey))
                {
                    return false;
                }

                if (currentKey.Key == Keys.Enter)
                {
                    Text = EditingText;
                    DisableKeyboard = true;
                    return true;
                }

                if (currentKey.Key == Keys.Escape)
                {
                    DisableKeyboard = true;
                    return true;
                }

                if (ProcessNavigationKeys(currentKey, newText))
                {
                    continue;
                }

                if ((MaxLength != 0 && newText.Length >= MaxLength))
                {
                    continue;
                }

                if (currentKey.Key == Keys.Space)
                {
                    newText.Insert(caretPos, ' ');
                }
                else if (currentKey.Character != 0)
                {
                    newText.Insert(caretPos, currentKey.Character);
                }
                else
                {
                    continue;
                }

                if (ValidateInput(newText))
                {
                    caretPos++;

                    if (caretPos > newText.Length)
                    {
                        caretPos = newText.Length;
                    }
                }
                else
                {
                    // undo edit
                    newText.Remove(caretPos, 1);
                }
            }

            EditingText = newText.ToString();

            return true;
        }

        bool ValidateInput(StringBuilder b)
        {
            if (!IsNumeric)
            {
                return true;
            }

            var s = b.ToString();
            if (string.IsNullOrWhiteSpace(s))
            {
                return true;
            }

            if (AllowDecimal)
            {
                return decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out var value);
            }
            else
            {
                return int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var value);
            }
        }

        bool ProcessNavigationKeys(AsciiKey currentKey, StringBuilder newText)
        {
            switch (currentKey.Key)
            {
                case Keys.Delete:
                {
                    if (caretPos != newText.Length)
                    {
                        newText.Remove(caretPos, 1);
                    }

                    if (caretPos > newText.Length)
                    {
                        caretPos = newText.Length;
                    }

                    return true;
                }
                case Keys.Back:
                {
                    if (newText.Length != 0 && caretPos != 0)
                    {
                        if (caretPos == newText.Length)
                        {
                            newText.Remove(newText.Length - 1, 1);
                        }
                        else
                        {
                            newText.Remove(caretPos - 1, 1);
                        }
                    }

                    caretPos -= 1;

                    if (caretPos < 0)
                    {
                        caretPos = 0;
                    }

                    return true;
                }
                case Keys.Left:
                {
                    caretPos -= 1;

                    if (caretPos == -1)
                    {
                        caretPos = 0;
                    }

                    return true;
                }
                case Keys.Right:
                {
                    caretPos += 1;

                    if (caretPos > newText.Length)
                    {
                        caretPos = newText.Length;
                    }

                    return true;
                }
                case Keys.Home:
                {
                    caretPos = 0;
                    return true;
                }
                case Keys.End:
                {
                    caretPos = newText.Length;
                    return true;
                }
                default:
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Called when the control loses focus.
        /// </summary>
        public override void FocusLost()
        {
            base.FocusLost();
            DisableKeyboard = true;
            Text = EditingText;
            IsDirty = true;
        }

        /// <summary>
        /// Called when the control is focused.
        /// </summary>
        public override void Focused()
        {
            base.Focused();
            DisableKeyboard = false;
            EditingText = text;
            IsDirty = true;
            ValidateCursorPosition();
        }

        protected override void OnLeftMouseClicked(MouseConsoleState state)
        {
            if (!DisableMouse)
            {
                base.OnLeftMouseClicked(state);

                DisableKeyboard = false;

                if (!IsFocused)
                {
                    Parent.FocusedControl = this;
                }

                IsDirty = true;
            }
        }

        [OnDeserialized]
        void AfterDeserialized(StreamingContext context)
        {
            Text = text;
            DetermineState();
            IsDirty = true;
        }

        /// <summary>
        /// Event arguments that indicate the change in text for a textbox control.
        /// </summary>
        public class TextChangedEventArgs : EventArgs
        {
            /// <summary>
            /// The original text value.
            /// </summary>
            public readonly string OldValue;

            /// <summary>
            /// The new text of the textbox.
            /// </summary>
            public string NewValue { get; set; }

            /// <summary>
            /// Creates a new event args object.
            /// </summary>
            /// <param name="oldValue">The original value of the text.</param>
            /// <param name="newValue">The value the text is chaning to.</param>
            public TextChangedEventArgs(string oldValue, string newValue)
            {
                OldValue = oldValue;
                NewValue = newValue;
            }
        }

        /// <summary>
        /// Event arguments to indicate that a key is being pressed on the textbox.
        /// </summary>
        public class KeyPressEventArgs : EventArgs
        {
            /// <summary>
            /// The key being pressed by the textbox.
            /// </summary>
            public readonly AsciiKey Key;

            /// <summary>
            /// When set to <see langword="true"/>, causes the textbox to cancel the key press.
            /// </summary>
            public bool IsCancelled { get; set; }

            /// <summary>
            /// Creates a new event args object.
            /// </summary>
            /// <param name="key">The key being pressed.</param>
            public KeyPressEventArgs(AsciiKey key) =>
                Key = key;
        }
    }
}
