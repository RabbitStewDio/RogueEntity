using Microsoft.Xna.Framework;
using RogueEntity.Api.Utils;
using SadConsole;

namespace RogueEntity.SadCons.MapRendering
{
    public readonly struct ConsoleRenderData
    {
        public readonly Optional<int> Glyph;
        public readonly Optional<Color> Foreground;
        public readonly Optional<Color> Background;
        public readonly bool GlyphImportant;
        public readonly bool ForegroundImportant;
        public readonly bool BackgroundImportant;

        public ConsoleRenderData(Optional<int> glyph = default,
                                 bool glyphImportant = false,
                                 Optional<Color> foreground = default,
                                 bool foregroundImportant = false,
                                 Optional<Color> background = default,
                                 bool backgroundImportant = false)
        {
            Glyph = glyph;
            Foreground = foreground;
            Background = background;
            GlyphImportant = glyphImportant;
            ForegroundImportant = foregroundImportant;
            BackgroundImportant = backgroundImportant;
        }

        public static ConsoleRenderData For(int glyph, bool important = false)
        {
            return new ConsoleRenderData(glyph, important);
        }

        public static ConsoleRenderData ForEmpty()
        {
            return new ConsoleRenderData();
        }

        public ConsoleRenderData WithForeground(Color color, bool important = false)
        {
            return new ConsoleRenderData(Glyph, GlyphImportant, color, important, Background, BackgroundImportant);
        }

        public ConsoleRenderData WithBackground(Color color, bool important = false)
        {
            return new ConsoleRenderData(Glyph, GlyphImportant, Foreground, ForegroundImportant, color, important);
        }

        public void ApplyTo(Cell c)
        {
            if (Glyph.TryGetValue(out var g))
            {
                c.Glyph = g;
            }

            if (Foreground.TryGetValue(out var fg))
            {
                c.Foreground = fg;
            }

            if (Background.TryGetValue(out var bg))
            {
                c.Background = bg;
            }
        }

        public ConsoleRenderData MergeWith(in ConsoleRenderData other)
        {
            var glyph = Merge(Glyph, GlyphImportant, other.Glyph, other.GlyphImportant);
            var bg = Merge(Background, BackgroundImportant, other.Background, other.BackgroundImportant);
            var fg = Merge(Foreground, ForegroundImportant, other.Foreground, other.ForegroundImportant);

            return new ConsoleRenderData(glyph.value, glyph.importance,
                                         fg.value, fg.importance,
                                         bg.value, bg.importance);
        }

        (Optional<TValue> value, bool importance) Merge<TValue>(in Optional<TValue> self, bool selfImportant, in Optional<TValue> other, bool otherImportant)
        {
            if (!other.HasValue)
            {
                return (self, selfImportant);
            }

            if (!self.HasValue)
            {
                return (other, otherImportant);
            }

            if (otherImportant)
            {
                return (other, true);
            }

            if (selfImportant)
            {
                return (self, true);
            }

            return (other, false);
        }
    }
}
