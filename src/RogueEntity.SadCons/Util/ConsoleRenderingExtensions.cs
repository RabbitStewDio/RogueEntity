using Microsoft.Xna.Framework;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Utils;

namespace RogueEntity.SadCons.Util
{
    public static class ConsoleRenderingExtensions
    {
        public static Color ToConsole(this Optional<RgbColor> color)
        {
            if (color.TryGetValue(out var rgb))
            {
                return new Color(rgb.Red, rgb.Green, rgb.Blue);
            }

            return Color.Transparent;
        }
    }
}