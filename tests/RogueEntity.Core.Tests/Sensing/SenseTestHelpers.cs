using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Tests.Sensing
{
    public static class SenseTestHelpers
    {

        public static DynamicBoolDataView2D ParseBool(string text) => ParseBool(text, out _);

        public static DynamicBoolDataView2D ParseBool(string text, out Rectangle parsedBounds)
        {
            var tmp = ParseMap(text, out parsedBounds);
            var result = new DynamicBoolDataView2D(tmp.OffsetX, tmp.OffsetY, tmp.TileSizeX, tmp.TileSizeY);
            result.ImportData(tmp, f => f > 0);
            return result;
        }
        public static DynamicDataView2D<float> ParseMap(string text) => ParseMap(text, out _);

        public static DynamicDataView2D<float> ParseMap(string text, out Rectangle parsedBounds)
        {
            var tokenParser = new TokenParser();
            tokenParser.AddToken("", 0f);
            tokenParser.AddToken(".", 0f);
            tokenParser.AddNumericToken<float>(TokenParser.ParseFloat);

            return TestHelpers.Parse<float>(text, tokenParser, out parsedBounds);
        }

        
        public static DynamicDataView2D<SenseDirectionStore> ParseDirections(string text) => ParseDirections(text, out _);

        public static DynamicDataView2D<SenseDirectionStore> ParseDirections(string text, out Rectangle parsedBounds)
        {
            var tokenParser = new TokenParser();
            foreach (var (key, value) in SenseDirectionsConsoleView.DirectionMappings)
            {
                tokenParser.AddToken(value, key);
            }
            tokenParser.AddToken("~", SenseDirection.None);
            tokenParser.AddToken("", SenseDirection.None);
            tokenParser.AddToken("#", SenseDataFlags.Obstructed);
            tokenParser.AddToken("*", SenseDataFlags.SelfIlluminating);
            tokenParser.Add(new SenseDirectionStoreTokenDefinition());
            return TestHelpers.Parse<SenseDirectionStore>(text, tokenParser, out parsedBounds);
        }

        class SenseDirectionStoreTokenDefinition : TokenParser.ITokenDefinition<SenseDirectionStore>
        {
            public int Weight => 2;
            
            public bool TryParse(string text, ITokenParser p, out SenseDirectionStore data)
            {
                var sd = SenseDirection.None;
                var sf = SenseDataFlags.None;
                foreach (var c in text)
                {
                    if (p.TryParse($"{c}", out SenseDirection d))
                    {
                        sd = d;
                    }

                    if (p.TryParse($"{c}", out SenseDataFlags f))
                    {
                        sf |= f;
                    }
                }
                
                data = SenseDirectionStore.From(sd, sf);
                return true;
            }
        }

        public static string PrintSenseDirectionStore(IReadOnlyView2D<SenseDirectionStore> map, Rectangle bounds)
        {
            return map.ExtendToString(bounds,
                                    elementSeparator: ",",
                                    elementStringifier: SenseDirectionStoreToString);

        }

        static string SenseDirectionStoreToString(SenseDirectionStore senseDirection)
        {
            if (senseDirection.Flags == default && senseDirection.Direction == default)
            {
                return "  ~ ";
            }

            var data = " " + SenseDirectionsConsoleView.DirectionMappings[senseDirection.Direction];
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
            return data;
        }
    }
}