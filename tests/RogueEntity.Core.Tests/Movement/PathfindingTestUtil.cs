using System.Collections.Generic;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.GoalFinding;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Tests.Movement
{
    public static class PathfindingTestUtil
    {
        public static DynamicDataView2D<float> ParseMap(string text) => ParseMap(text, out _);

        public static DynamicDataView2D<float> ParseMap(string text, out Rectangle parsedBounds)
        {
            var tokenParser = new TokenParser();
            tokenParser.AddToken("", 1f);
            tokenParser.AddToken(".", 1f);
            tokenParser.AddToken("###", 0f);
            tokenParser.AddToken("##", 0f);
            tokenParser.AddToken("#", 0f);
            tokenParser.AddNumericToken<float>(TokenParser.ParseFloat);

            return TestHelpers.Parse<float>(text, tokenParser, out parsedBounds);
        }

        public static DynamicDataView2D<(float, Optional<GoalMarker<TGoal>>)> ParseGoalMap<TGoal>(string text, out Rectangle parsedBounds)
        {
            static bool ParseFloat(float f, out (float, Optional<GoalMarker<TGoal>>) r)
            {
                var none = Optional.Empty<GoalMarker<TGoal>>();
                r = (f, none);
                return true;
            }

            var none = Optional.Empty<GoalMarker<TGoal>>();
            
            var tokenParser = new TokenParser();
            tokenParser.AddToken("", (1f, none));
            tokenParser.AddToken(".", (1f, none));
            tokenParser.AddToken("###", (0f, none));
            tokenParser.AddToken("##", (0f, none));
            tokenParser.AddToken("#", (0f, none));
            tokenParser.AddToken("G", (0f, Optional.ValueOf(new GoalMarker<TGoal>(10))));
            tokenParser.AddNumericToken<(float, Optional<GoalMarker<TGoal>>)>(ParseFloat);

            return TestHelpers.Parse<(float, Optional<GoalMarker<TGoal>>)>(text, tokenParser, out parsedBounds);
        }

        public static DynamicDataView2D<(bool, int)> ParseResultMap(string text, out Rectangle parsedBounds)
        {
            var tokenParser = new TokenParser();
            tokenParser.AddToken("", (false, -1));
            tokenParser.AddToken(".", (false, -1));
            tokenParser.AddToken("###", (true, -1));
            tokenParser.AddToken("##", (true, -1));
            tokenParser.AddToken("#", (true, -1));
            tokenParser.AddToken("@", (false, 0));
            tokenParser.AddNumericToken<(bool, int)>(ParsePathResult);

            return TestHelpers.Parse<(bool, int)>(text, tokenParser, out parsedBounds);
        }

        static bool ParsePathResult(float parsedNumber, out (bool, int) result)
        {
            result = (false, (int)parsedNumber);
            return true;
        }

        public static string PrintResultMap(IReadOnlyView2D<(bool, int)> resultMap, Rectangle bounds)
        {
            return resultMap.ExtendToString(bounds,
                                            elementSeparator: ",",
                                            elementStringifier: PathFinderResultToString);
        }

        static string PathFinderResultToString((bool, int) arg)
        {

            if (arg.Item2 < 0)
            {
                if (arg.Item1)
                {
                    return " ### ";
                }
                
                return "  .  ";
            }
            
            if (arg.Item2 == 0)
            {
                return "  @  ";
            }

            return $" {arg.Item2,3} ";
        }
        
        public static DynamicDataView2D<(bool, int)> CreateResult(DynamicDataView2D<float> resistanceMap,
                                                                  List<(EntityGridPosition, IMovementMode)> resultPath,
                                                                  EntityGridPosition startPos,
                                                                  Rectangle bounds)
        {
            var resultMap = new DynamicDataView2D<(bool, int)>(resistanceMap.ToConfiguration());

            foreach (var (x, y) in bounds.Contents)
            {
                var wall = resistanceMap[x, y] <= 0;
                var pos = startPos.WithPosition(x, y);
                int pathIndex;
                if (pos == startPos)
                {
                    pathIndex = 0;
                }
                else
                {
                    pathIndex = resultPath.PathIndexOf(pos);
                    if (pathIndex >= 0)
                    {
                        pathIndex += 1;
                    }
                }

                resultMap[x, y] = (wall, pathIndex);
            }

            return resultMap;
        }

    }
}