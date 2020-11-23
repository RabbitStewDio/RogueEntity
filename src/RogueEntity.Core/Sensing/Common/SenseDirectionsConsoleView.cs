using System.Collections.Generic;

namespace RogueEntity.Core.Sensing.Common
{
    public static class SenseDirectionsConsoleView
    {
        public static readonly IReadOnlyDictionary<SenseDirection, string> DirectionMappings;

        static SenseDirectionsConsoleView()
        {
            DirectionMappings = new Dictionary<SenseDirection, string>
            {
                [SenseDirection.North | SenseDirection.West] = "\u250c", // Box-Drawing Light Down and Right
                [SenseDirection.North] = "\u252c", // Box-Drawing Light Horizontal and Down
                [SenseDirection.North | SenseDirection.East] = "\u2510", // Box-Drawing Light Down and Left

                [SenseDirection.West] = "\u251c", // Box-Drawing Light Vertical and Right
                [SenseDirection.None] = "\u253c", // Box-Drawing Light Vertical and Horizontal
                [SenseDirection.East] = "\u2524", // Box-Drawing Light Vertical and Left

                [SenseDirection.South | SenseDirection.West] = "\u2514", // Box-Drawing Light Up and Right
                [SenseDirection.South] = "\u2534", // Box-Drawing Light Up and Horizontal
                [SenseDirection.South | SenseDirection.East] = "\u2518" // Box-Drawing Light Up and Left 
            };
        }
    }
}