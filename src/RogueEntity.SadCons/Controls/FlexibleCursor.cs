using JetBrains.Annotations;
using SadConsole;
using SadConsole.Effects;
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SadConsole.StringParser;
using System.Threading;

namespace RogueEntity.SadCons.Controls
{
    /// <summary>
    /// A cursor that is attached to a <see cref="SadConsole.Console"/> used for printing.
    /// </summary>
    public class FlexibleCursor
    {
        static readonly int DefaultCursorCharacter = 219;
        
        CellSurface editor;
        Cell cursorRenderCell;
        Point position;
        Rectangle bounds;


        /// <summary>
        /// Cell used to render the cursor on the screen.
        /// </summary>
        public Cell CursorRenderCell
        {
            get => cursorRenderCell;
            set
            {
                CursorEffect?.ClearCell(cursorRenderCell);

                cursorRenderCell = value ?? throw new NullReferenceException("The render cell cannot be null. To hide the cursor, use the IsVisible property.");

                CursorEffect?.AddCell(cursorRenderCell);
            }
        }

        /// <summary>
        /// Appearance used when printing text.
        /// </summary>
        public Cell PrintAppearance { get; set; }

        /// <summary>
        /// This effect is applied to each cell printed by the cursor.
        /// </summary>
        public ICellEffect PrintEffect { get; set; }

        /// <summary>
        /// This is the cursor visible effect, like blinking.
        /// </summary>
        public ICellEffect CursorEffect { get; set; }

        /// <summary>
        /// When true, indicates that the cursor, when printing, should not use the <see cref="PrintAppearance"/> property in determining the color/effect of the cell, but keep the cell the same as it was.
        /// </summary>
        public bool PrintOnlyCharacterData { get; set; }

        /// <summary>
        /// Shows or hides the cursor. This does not affect how the cursor operates.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// When true, allows the <see cref="ProcessKeyboard"/> method to run.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the location of the cursor on the console.
        /// </summary>
        public Point Position
        {
            get => position;
            set
            {
                if (editor != null)
                {
                    Point old = position;

                    if (!(value.X < 0 || value.X >= bounds.Width))
                    {
                        position.X = value.X;
                    }

                    if (!(value.Y < 0 || value.Y >= bounds.Height))
                    {
                        position.Y = value.Y;
                    }

                    if (position != old)
                    {
                        editor.IsDirty = true;
                    }
                }
            }
        }

        /// <summary>
        /// When true, prevents the any print method from breaking words up by spaces when wrapping lines.
        /// </summary>
        public bool DisableWordBreak { get; set; }

        /// <summary>
        /// Enables linux-like string parsing where a \n behaves like a \r\n.
        /// </summary>
        public bool UseLinuxLineEndings { get; set; }

        /// <summary>
        /// Calls <see cref="ColoredString.Parse"/> to create a colored string when using <see cref="Print(string)"/> or <see cref="Print(string, Cell, ICellEffect)"/>
        /// </summary>
        public bool UseStringParser { get; set; }

        /// <summary>
        /// Gets or sets the row of the cursor position.
        /// </summary>
        public int Row
        {
            get => position.Y;
            set => Position = new Point(Column, value);
        }

        /// <summary>
        /// Gets or sets the column of the cursor position.
        /// </summary>
        public int Column
        {
            get => position.X;
            set => Position = new Point(value, Row);
        }

        /// <summary>
        /// Indicates that the when the cursor goes past the last cell of the console, that the rows should be shifted up when the cursor is automatically reset to the next line.
        /// </summary>
        public bool AutomaticallyShiftRowsUp { get; set; }

        /// <summary>
        /// Creates a new instance of the cursor class that will work with the specified console.
        /// </summary>
        /// <param name="console">The console this cursor will print on.</param>
        public FlexibleCursor(CellSurface console) : this(console, new Rectangle(0, 0, console.Width, console.Height)) {}
        
        public FlexibleCursor(CellSurface console, Rectangle bounds)
        {
            this.editor = console;
            this.bounds = Rectangle.Intersect(bounds, new Rectangle(0, 0, console.Width, console.Height));

            IsEnabled = true;
            IsVisible = false;
            AutomaticallyShiftRowsUp = true;

            PrintAppearance = new Cell(Color.White, Color.Black, 0);
            CursorRenderCell = new Cell(Color.White, Color.Transparent, DefaultCursorCharacter);

            ResetCursorEffect();
        }

        public FlexibleCursor()
        {
            IsEnabled = true;
            IsVisible = false;
            AutomaticallyShiftRowsUp = true;

            PrintAppearance = new Cell(Color.White, Color.Black, 0);
            CursorRenderCell = new Cell(Color.White, Color.Transparent, DefaultCursorCharacter);

            ResetCursorEffect();
        }

        /// <summary>
        /// Sets the console this cursor is targeting.
        /// </summary>
        /// <param name="console">The console the cursor works with.</param>
        public void AttachSurface([NotNull] CellSurface console)
        {
            this.editor = console ?? throw new ArgumentNullException(nameof(console));
            this.bounds = new Rectangle(0, 0, console.Width, console.Height);
            Position = Position;
        }

        /// <summary>
        /// Sets the console this cursor is targeting.
        /// </summary>
        /// <param name="console">The console the cursor works with.</param>
        /// <param name="bounds"></param>
        public void AttachSurface([NotNull] CellSurface console, Rectangle bounds)
        {
            this.editor = console ?? throw new ArgumentNullException(nameof(console));
            this.bounds = Rectangle.Intersect(bounds, new Rectangle(0, 0, console.Width, console.Height));
            Position = Position;
        }

        public void DetachSurface()
        {
            this.editor = default;
            this.bounds = default;
            this.position = default;
        }
        
        /// <summary>
        /// Resets the <see cref="CursorRenderCell"/> back to the default.
        /// </summary>
        public FlexibleCursor ResetCursorEffect()
        {
            var blinkEffect = new Blink
            {
                BlinkSpeed = 0.35f
            };
            CursorEffect = blinkEffect;
            CursorEffect.AddCell(CursorRenderCell);
            return this;
        }

        /// <summary>
        /// Resets the cursor appearance to the console's default foreground and background.
        /// </summary>
        /// <returns>This cursor object.</returns>
        /// <exception cref="Exception">Thrown when the backing console's CellData is null.</exception>
        public FlexibleCursor ResetAppearanceToConsole()
        {
            if (editor != null)
            {
                PrintAppearance = new Cell(editor.DefaultForeground, editor.DefaultBackground, 0);
            }
            else
            {
                throw new Exception("Attached console is null. Cannot reset appearance.");
            }

            return this;
        }

        /// <summary>
        /// Sets <see cref="PrintAppearance"/>.
        /// </summary>
        /// <param name="appearance">The appearance to set.</param>
        /// <returns>This cursor object.</returns>
        public FlexibleCursor SetPrintAppearance(Cell appearance)
        {
            PrintAppearance = appearance;
            return this;
        }
        
        void PrintGlyph(ColoredGlyph glyph, ColoredString settings)
        {
            var editorPosX = position.X + bounds.X;
            var editorPosY = position.Y + bounds.Y;
            Cell cell = editor.Cells[editorPosY * editor.Width + editorPosX];

            if (!PrintOnlyCharacterData)
            {
                if (!settings.IgnoreGlyph)
                {
                    cell.Glyph = glyph.GlyphCharacter;
                }

                if (!settings.IgnoreBackground)
                {
                    cell.Background = glyph.Background;
                }

                if (!settings.IgnoreForeground)
                {
                    cell.Foreground = glyph.Foreground;
                }

                if (!settings.IgnoreMirror)
                {
                    cell.Mirror = glyph.Mirror;
                }

                if (!settings.IgnoreEffect)
                {
                    editor.SetEffect(cell, glyph.Effect);
                }
            }
            else if (!settings.IgnoreGlyph)
            {
                cell.Glyph = glyph.GlyphCharacter;
            }

            position.X += 1;
            if (position.X >= bounds.Width)
            {
                CarriageReturn().LineFeed(AutomaticallyShiftRowsUp);
            }

            editor.IsDirty = true;
        }

        /// <summary>
        /// Prints text to the console using the default print appearance.
        /// </summary>
        /// <param name="text">The text to print.</param>
        /// <returns>Returns this cursor object.</returns>
        public FlexibleCursor Print(string text)
        {
            Print(text, PrintAppearance, PrintEffect);
            return this;
        }

        /// <summary>
        /// Prints text on the console.
        /// </summary>
        /// <param name="text">The text to print.</param>
        /// <param name="template">The way the text will look when it is printed.</param>
        /// <param name="templateEffect">Effect to apply to the text as its printed.</param>
        /// <returns>Returns this cursor object.</returns>
        public FlexibleCursor Print(string text, Cell template, ICellEffect templateEffect)
        {
            var coloredString = PrepareColoredString(text, template, templateEffect);
            return Print(coloredString);
        }

        public ColoredString PrepareColoredString(string text, Cell template = null, ICellEffect templateEffect = null)
        {
            template ??= PrintAppearance;
            templateEffect ??= PrintEffect;
            ColoredString coloredString;
            var stack = ParseCommandStacksThreadVar.Value;
            try
            {
                if (UseStringParser)
                {
                    var editorPosX = position.X + bounds.X;
                    var editorPosY = position.Y + bounds.Y;
                    coloredString = ColoredString.Parse(text, editorPosY * editor.Width + editorPosX, editor, stack);
                }
                else
                {
                    coloredString = text.CreateColored(stack, template.Foreground, template.Background, template.Mirror);
                    coloredString.SetEffect(templateEffect);
                }
            }
            finally
            {
                stack.Clear();
            }

            return coloredString;
        }

        static readonly ThreadLocal<ColoredGlyph> SpaceGlyphThreadVar = new ThreadLocal<ColoredGlyph>(() => new ColoredGlyph(), true);
        static readonly ThreadLocal<ParseCommandStacks> ParseCommandStacksThreadVar = new ThreadLocal<ParseCommandStacks>(() => new ParseCommandStacks(), true);
        
        public FlexibleCursor Print(ColoredString coloredString)
        {
            int wordStart = 0;
            int wordEnd = 0;

            var spaceGlyph = SpaceGlyphThreadVar.Value;
            var ignoreCarriageReturn = false;
            var previousWasCarriageReturn = false;

            bool containsNonWhiteSpace = false;
            for (var index = 0; index < coloredString.Count; index++)
            {
                var c = coloredString[index];
                if (!char.IsWhiteSpace(c.GlyphCharacter))
                {
                    containsNonWhiteSpace = true;
                    wordEnd = index + 1;
                    previousWasCarriageReturn = false;
                    continue;
                }

                if (containsNonWhiteSpace)
                {
                    var wordSize = wordEnd - wordStart;
                    var availableSpaceOnLine = bounds.Width - position.X;
                    if (!DisableWordBreak && wordSize > availableSpaceOnLine)
                    {
                        spaceGlyph.CopyAppearanceFrom(c);
                        spaceGlyph.GlyphCharacter = ' ';
                        var padding = wordSize - availableSpaceOnLine;
                        for (var p = 0; p < padding; p += 1)
                        {
                            PrintGlyph(spaceGlyph, coloredString);
                        }
                    }

                    while (wordStart != wordEnd)
                    {
                        for (; wordStart < wordEnd; wordStart += 1)
                        {
                            PrintGlyph(coloredString[wordStart], coloredString);
                        }
                    }

                    containsNonWhiteSpace = false;
                }

                if (c.GlyphCharacter == '\n')
                {
                    LineFeed(AutomaticallyShiftRowsUp);
                    if (!previousWasCarriageReturn && UseLinuxLineEndings)
                    {
                        CarriageReturn();
                    }

                    ignoreCarriageReturn = true;
                    continue;
                }

                if (c.GlyphCharacter == '\r')
                {
                    if (ignoreCarriageReturn)
                    {
                        ignoreCarriageReturn = false;
                    }
                    else
                    {
                        CarriageReturn();
                    }
                    previousWasCarriageReturn = true;
                    continue;
                }
                else
                {
                    previousWasCarriageReturn = false;
                    ignoreCarriageReturn = false;
                }

                if (c.GlyphCharacter == '\t')
                {
                    // go to next tab position
                    position.X = ((position.X + 8) / 8) * 8;
                    if (position.X > bounds.Width)
                    {
                        CarriageReturn().LineFeed(AutomaticallyShiftRowsUp);
                    }
                    continue;
                }

                PrintGlyph(c, coloredString);
                position.X += 1;
                wordStart = index + 1;
            }

            // Handle straggling characters that may be left behind from the loop above.
            if (containsNonWhiteSpace)
            {
                var wordSize = wordEnd - wordStart;
                var availableSpaceOnLine = bounds.Width - position.X;
                if (!DisableWordBreak && wordSize > availableSpaceOnLine)
                {
                    spaceGlyph.CopyAppearanceFrom(coloredString[wordStart]);
                    spaceGlyph.GlyphCharacter = ' ';
                    var padding = wordSize - availableSpaceOnLine;
                    for (var p = 0; p < padding; p += 1)
                    {
                        PrintGlyph(spaceGlyph, coloredString);
                    }
                    CarriageReturn().LineFeed(AutomaticallyShiftRowsUp);
                }

                while (wordStart != wordEnd)
                {
                    for (; wordStart < wordEnd; wordStart += 1)
                    {
                        PrintGlyph(coloredString[wordStart], coloredString);
                    }
                }
            }

            return this;
        }
        
        /// <summary>
        /// Returns the cursor to the start of the current line.
        /// </summary>
        /// <returns>The current cursor object.</returns>
        public FlexibleCursor CarriageReturn()
        {
            position.X = 0;
            return this;
        }

        /// <summary>
        /// Moves the cursor down a line.
        /// </summary>
        /// <returns>The current cursor object.</returns>
        public FlexibleCursor LineFeed()
        {
            return LineFeed(true);
        }

        public FlexibleCursor LineFeed(bool forceShiftUp)
        {
            if (position.Y == bounds.Height - 1)
            {
                if (forceShiftUp)
                {
                    editor.ShiftUp(bounds);
                }
            }
            else
            {
                position.Y++;
            }

            return this;
        }

        /// <summary>
        /// Calls the <see cref="CarriageReturn"/> and <see cref="LineFeed"/> methods in a single call.
        /// </summary>
        /// <returns>The current cursor object.</returns>
        public FlexibleCursor NewLine() => CarriageReturn().LineFeed();

        /// <summary>
        /// Moves the cursor to the specified position.
        /// </summary>
        /// <param name="position">The destination of the cursor.</param>
        /// <returns>This cursor object.</returns>
        public FlexibleCursor Move(Point position)
        {
            Position = position;
            return this;
        }


        /// <summary>
        /// Moves the cursor to the specified position.
        /// </summary>
        /// <param name="x">The x (horizontal) of the position.</param>
        /// <param name="y">The x (vertical) of the position.</param>
        /// <returns></returns>
        public FlexibleCursor Move(int x, int y)
        {
            Position = new Point(x, y);
            return this;
        }

        /// <summary>
        /// Moves the cusor up by the specified amount of lines.
        /// </summary>
        /// <param name="amount">The amount of lines to move the cursor</param>
        /// <returns>This cursor object.</returns>
        public FlexibleCursor Up(int amount)
        {
            int newY = position.Y - amount;

            if (newY < 0)
            {
                newY = 0;
            }

            Position = new Point(position.X, newY);
            return this;
        }

        /// <summary>
        /// Moves the cusor down by the specified amount of lines.
        /// </summary>
        /// <param name="amount">The amount of lines to move the cursor</param>
        /// <returns>This cursor object.</returns>
        public FlexibleCursor Down(int amount)
        {
            int newY = position.Y + amount;

            if (newY >= bounds.Height)
            {
                newY = bounds.Height - 1;
            }

            Position = new Point(position.X, newY);
            return this;
        }

        /// <summary>
        /// Moves the cusor left by the specified amount of columns.
        /// </summary>
        /// <param name="amount">The amount of columns to move the cursor</param>
        /// <returns>This cursor object.</returns>
        public FlexibleCursor Left(int amount)
        {
            int newX = position.X - amount;

            if (newX < 0)
            {
                newX = 0;
            }

            Position = new Point(newX, position.Y);
            return this;
        }

        /// <summary>
        /// Moves the cusor left by the specified amount of columns, wrapping the cursor if needed.
        /// </summary>
        /// <param name="amount">The amount of columns to move the cursor</param>
        /// <returns>This cursor object.</returns>
        public FlexibleCursor LeftWrap(int amount)
        {
            int index = Helpers.GetIndexFromPoint(position.X, position.Y, bounds.Width) - amount;

            if (index < 0)
            {
                index = 0;
            }

            position = Helpers.GetPointFromIndex(index, bounds.Width);
            return this;
        }

        /// <summary>
        /// Moves the cusor right by the specified amount of columns.
        /// </summary>
        /// <param name="amount">The amount of columns to move the cursor</param>
        /// <returns>This cursor object.</returns>
        public FlexibleCursor Right(int amount)
        {
            int newX = position.X + amount;

            if (newX >= bounds.Width)
            {
                newX = bounds.Width - 1;
            }

            Position = new Point(newX, position.Y);
            return this;
        }

        /// <summary>
        /// Moves the cusor right by the specified amount of columns, wrapping the cursor if needed.
        /// </summary>
        /// <param name="amount">The amount of columns to move the cursor</param>
        /// <returns>This cursor object.</returns>
        public FlexibleCursor RightWrap(int amount)
        {
            int index = Helpers.GetIndexFromPoint(position.X, position.Y, bounds.Width) - amount;
            var cells = bounds.Width * bounds.Height;
            if (index > editor.Cells.Length)
            {
                index %= cells;
            }

            position = Helpers.GetPointFromIndex(index, bounds.Width);

            return this;
        }

        /// <inheritdoc />
        public virtual void Render(SpriteBatch batch, Font font, Rectangle renderArea)
        {
            batch.Draw(font.FontImage, renderArea, font.GlyphRects[font.SolidGlyphIndex], CursorRenderCell.Background, 0f, Vector2.Zero, SpriteEffects.None, 0.6f);
            batch.Draw(font.FontImage, renderArea, font.GlyphRects[CursorRenderCell.Glyph], CursorRenderCell.Foreground, 0f, Vector2.Zero, SpriteEffects.None, 0.7f);
        }

        internal void Update(TimeSpan elapsed)
        {
            if (CursorEffect != null)
            {
                CursorEffect.Update(elapsed.TotalSeconds);

                if (CursorEffect.UpdateCell(CursorRenderCell))
                {
                    editor.IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Automates the cursor based on keyboard input.
        /// </summary>
        /// <param name="info">The state of the keyboard</param>
        /// <returns>Returns true when the keyboard caused the cursor to do something.</returns>
        public virtual bool ProcessKeyboard(SadConsole.Input.Keyboard info)
        {
            if (!IsEnabled)
            {
                return false;
            }

            bool didSomething = false;

            foreach (SadConsole.Input.AsciiKey key in info.KeysPressed)
            {
                if (key.Character == '\0')
                {
                    switch (key.Key)
                    {
                        case Keys.Space:
                            Print(key.Character.ToString());
                            didSomething = true;
                            break;
                        case Keys.Enter:
                            CarriageReturn().LineFeed();
                            didSomething = true;
                            break;

                        case Keys.Pause:
                        case Keys.Escape:
                        case Keys.F1:
                        case Keys.F2:
                        case Keys.F3:
                        case Keys.F4:
                        case Keys.F5:
                        case Keys.F6:
                        case Keys.F7:
                        case Keys.F8:
                        case Keys.F9:
                        case Keys.F10:
                        case Keys.F11:
                        case Keys.F12:
                        case Keys.CapsLock:
                        case Keys.NumLock:
                        case Keys.PageUp:
                        case Keys.PageDown:
                        case Keys.Home:
                        case Keys.End:
                        case Keys.LeftShift:
                        case Keys.RightShift:
                        case Keys.LeftAlt:
                        case Keys.RightAlt:
                        case Keys.LeftControl:
                        case Keys.RightControl:
                        case Keys.LeftWindows:
                        case Keys.RightWindows:
                        case Keys.F13:
                        case Keys.F14:
                        case Keys.F15:
                        case Keys.F16:
                        case Keys.F17:
                        case Keys.F18:
                        case Keys.F19:
                        case Keys.F20:
                        case Keys.F21:
                        case Keys.F22:
                        case Keys.F23:
                        case Keys.F24:
                            //this._virtualCursor.Print(key.Character.ToString());
                            break;
                        case Keys.Up:
                            Up(1);
                            didSomething = true;
                            break;
                        case Keys.Left:
                            Left(1);
                            didSomething = true;
                            break;
                        case Keys.Right:
                            Right(1);
                            didSomething = true;
                            break;
                        case Keys.Down:
                            Down(1);
                            didSomething = true;
                            break;
                        case Keys.None:
                            break;
                        case Keys.Back:
                            Left(1).Print(" ").Left(1);
                            didSomething = true;
                            break;
                        default:
                            Print(key.Character.ToString());
                            didSomething = true;
                            break;
                    }
                }
                else
                {
                    Print(key.Character.ToString());
                    didSomething = true;
                }
            }

            return didSomething;
        }

    }
}
