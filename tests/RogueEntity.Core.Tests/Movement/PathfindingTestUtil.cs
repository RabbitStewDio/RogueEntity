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

        static bool IsWall(string text)
        {
            return "".PadLeft(text.Length, '#').Equals(text);
        }
        
    }
}