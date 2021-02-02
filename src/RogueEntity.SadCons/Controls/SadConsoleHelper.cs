using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEntity.Api.Utils;
using SadConsole;
using SadConsole.StringParser;
using System.Buffers;
using System.Threading;

namespace RogueEntity.SadCons.Controls
{
    public static class SadConsoleHelper
    {
        public static void Clear(this ParseCommandStacks s)
        {
            s.All.Clear();
            s.Background.Clear();
            s.Effect.Clear();
            s.Foreground.Clear();
            s.Glyph.Clear();
            s.Mirror.Clear();
            s.TurnOnEffects = false;
        }
        
        readonly struct CommandHolder
        {
            readonly ParseCommandRecolor Foreground;
            readonly ParseCommandRecolor Background;
            readonly ParseCommandMirror Mirror;

            public CommandHolder(ParseCommandRecolor foreground, ParseCommandRecolor background, ParseCommandMirror mirror)
            {
                Foreground = foreground;
                Background = background;
                Mirror = mirror;
            }

            public void Populate(ParseCommandStacks stacks,
                                 Optional<Color> foreground = default,
                                 Optional<Color> background = default,
                                 Optional<SpriteEffects> mirror = default)
            {
                if (foreground.TryGetValue(out var fg))
                {
                    Foreground.R = fg.R;
                    Foreground.G = fg.G;
                    Foreground.B = fg.B;
                    Foreground.CommandType = CommandTypes.Foreground;
                    stacks.AddSafe(Foreground);
                }

                if (background.TryGetValue(out var bg))
                {
                    Background.R = bg.R;
                    Background.G = bg.G;
                    Background.B = bg.B;
                    Background.CommandType = CommandTypes.Background;
                    stacks.AddSafe(Background);
                }

                if (mirror.TryGetValue(out var m))
                {
                    Mirror.Mirror = m;
                    Mirror.CommandType = CommandTypes.Mirror;
                    stacks.AddSafe(Mirror);
                }
            }

            public static CommandHolder Create()
            {
                return new CommandHolder(new ParseCommandRecolor(), new ParseCommandRecolor(), new ParseCommandMirror());
            }
        }

        static readonly ThreadLocal<CommandHolder> CommandCache = new ThreadLocal<CommandHolder>(CommandHolder.Create);


        /// <summary>
        /// Creates a <see cref="ColoredString"/> object from an existing string with the specified foreground and background, setting the ignore properties if needed.
        /// </summary>
        /// <param name="value">The current string.</param>
        /// <param name="stacks"></param>
        /// <param name="foreground">The foreground color. If null, <see cref="ColoredString.IgnoreForeground"/> will be set.</param>
        /// <param name="background">The background color. If null, <see cref="ColoredString.IgnoreBackground"/> will be set.</param>
        /// <param name="mirror">The mirror setting. If null, <see cref="ColoredString.IgnoreMirror"/> will be set.</param>
        /// <returns>A <see cref="ColoredString"/> object instance.</returns>
        public static ColoredString CreateColored(this string value,
                                                  ParseCommandStacks stacks,
                                                  Optional<Color> foreground = default,
                                                  Optional<Color> background = default,
                                                  Optional<SpriteEffects> mirror = default)
        {
            CommandCache.Value.Populate(stacks, foreground, background, mirror);
            ColoredString newString = ColoredString.Parse(value, initialBehaviors: stacks);
            if (!foreground.HasValue)
            {
                newString.IgnoreForeground = true;
            }

            if (!background.HasValue)
            {
                newString.IgnoreBackground = true;
            }

            if (!mirror.HasValue)
            {
                newString.IgnoreMirror = true;
            }

            return newString;
        }
        
        public static SadConsole.Game SadConsoleGameInstance => SadConsole.Game.Instance as SadConsole.Game;

        public static void ShiftUp(this CellSurface s, Rectangle bounds, bool wrap = false)
        {
            var realBounds = Rectangle.Intersect(bounds, new Rectangle(0, 0, s.Width, s.Height));
            if (realBounds.Width == 0 || realBounds.Height == 0)
            {
                return;
            }
            
            var cells = s.Cells;
            var wrappedLine = ArrayPool<Cell>.Shared.Rent(realBounds.Width);
            try
            {
                if (wrap)
                {
                    var idx = realBounds.Y * s.Width + realBounds.X;
                    for (int i = 0; i < realBounds.Width; i += 1)
                    {
                        var cell = new Cell();
                        cells[idx + i].CopyAppearanceTo(cell);
                        wrappedLine[i] = cell;
                    }
                }

                var lastLine = realBounds.Y + realBounds.Height - 1;
                for (int y = realBounds.Y; y < lastLine; y += 1)
                {
                    var targetIndex = y * s.Width + realBounds.X;
                    var sourceIndex = (y + 1) * s.Width + realBounds.X;

                    for (int i = 0; i < realBounds.Width; i += 1)
                    {
                        var sourceCell = cells[sourceIndex + i];
                        cells[targetIndex + i].CopyAppearanceFrom(sourceCell);
                    }
                }

                var lastLineIndex = (lastLine) * s.Width + realBounds.X;
                if (wrap)
                {
                    for (int i = 0; i < realBounds.Width; i += 1)
                    {
                        cells[lastLineIndex + i].CopyAppearanceFrom(wrappedLine[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < realBounds.Width; i += 1)
                    {
                        cells[lastLineIndex + 1].Clear();
                    }

                }

                s.ShiftUp();
            }
            finally
            {
                ArrayPool<Cell>.Shared.Return(wrappedLine, true);
            }
        }
        
        
    }
}
