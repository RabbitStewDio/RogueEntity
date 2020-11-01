using System;
using System.Collections.Generic;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Tests.Sensing.Common
{
    public class SenseMapDirectionTestView : IReadOnlyView2D<string>
    {
        readonly Dictionary<SenseDirection, string> directionMapping;
        readonly ISenseDataView backend;

        public SenseMapDirectionTestView(ISenseDataView backend)
        {
            this.backend = backend;
            this.directionMapping = new Dictionary<SenseDirection, string>
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

        public bool TryGet(int x, int y, out string data)
        {
            if (backend.TryQuery(x, y, out var intensity, out var senseDirection))
            {
                
                if (Math.Abs(intensity) < 0.0005f)
                {
                    data = "  ~ ";
                    return false;
                }
                
                data = " " + directionMapping[senseDirection.Direction];
                if (senseDirection.Flags.HasFlags(SenseDataFlags.Obstructed))
                {
                    data += "#";
                }
                else
                {
                    data += " ";
                }
                if (senseDirection.Flags.HasFlags(SenseDataFlags.SelfIlluminating))
                {
                    data += "*";
                } 
                else
                {
                    data += " ";
                }
                
                return true;
            }
 
            data = "  ~ ";
            return false;
        }

        public string this[int x, int y]
        {
            get
            {
                if (TryGet(x, y, out var result))
                {
                    return result;
                }

                return "  ~ ";
            }
        }
    }
}