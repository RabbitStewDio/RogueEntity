using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Tests.Players
{
    public enum PlayerTestEntitySymbols
    {
        Empty, Wall, SpawnLocation
    }
    
    public static class PlayerTestUtil
    {
        
        public static DynamicDataView2D<PlayerTestEntitySymbols> ParseMap(string text, out Rectangle parsedBounds)
        {
            var tokenParser = new TokenParser();
            tokenParser.AddToken("", PlayerTestEntitySymbols.Empty);
            tokenParser.AddToken(".", PlayerTestEntitySymbols.Empty);
            tokenParser.AddToken("###", PlayerTestEntitySymbols.Wall);
            tokenParser.AddToken("##", PlayerTestEntitySymbols.Wall);
            tokenParser.AddToken("#", PlayerTestEntitySymbols.Wall);
            tokenParser.AddToken("$", PlayerTestEntitySymbols.SpawnLocation);

            return TestHelpers.Parse<PlayerTestEntitySymbols>(text, tokenParser, out parsedBounds);
        }

    }
}
