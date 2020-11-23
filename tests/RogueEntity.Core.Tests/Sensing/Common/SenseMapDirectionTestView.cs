using System;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Tests.Sensing.Common
{
    public class SenseMapDirectionTestView : IReadOnlyView2D<string>
    {
        readonly ISenseDataView backend;

        public SenseMapDirectionTestView(ISenseDataView backend)
        {
            this.backend = backend;
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

                data = " " + SenseDirectionsConsoleView.DirectionMappings[senseDirection.Direction];
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