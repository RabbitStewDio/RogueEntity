using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using RogueEntity.Core.Tests;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Performance.Tests
{
    public static class PerformanceTestUtils
    {
        public static string ReadResource(string id)
        {
            var assembly = typeof(PerformanceTestUtils).GetTypeInfo().Assembly;
            using var resource = assembly.GetManifestResourceStream("RogueEntity.Performance.Tests." + id);
            if (resource == null)
            {
                throw new MissingManifestResourceException();
            }
            
            using var r = new StreamReader(resource, Encoding.ASCII);
            return r.ReadToEnd();
        }
        
        public static DynamicDataView2D<float> ParseMap(string text, out Rectangle parsedBounds)
        {
            var tokenParser = new TokenParser();
            tokenParser.AddToken("..", 1f);
            tokenParser.AddToken("##", 0f);
            tokenParser.AddNumericToken<float>(TokenParser.ParseFloat);

            return ParseDenseMap<float>(text, tokenParser, out parsedBounds);
        }

        public static DynamicDataView2D<TData> ParseDenseMap<TData>(string text, TokenParser tokenParser, out Rectangle parsedBounds)
        {
            var map = new DynamicDataView2D<TData>(0, 0, 64, 64);
            var row = -1;
            using var sr = new StringReader(text);

            var maxX = 0;
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                line = line.Trim();
                if (line.StartsWith("//"))
                {
                    // allow comments. I am not a monster.
                    continue;
                }

                row += 1;
                var maxLength = (line.Length / 2) * 2;
                for (var index = 0; index < line.Length; index += 2)
                {
                    
                    var v = $"{line[index]}{line[index + 1]}";
                    if (!tokenParser.TryParse(v, out TData result))
                    {
                        throw new Exception($"Unable to parse token {v} as {typeof(TData)}");
                    }

                    map[index / 2, row] = result;
                    maxX = Math.Max(maxX, index / 2);
                }
            }

            parsedBounds = new Rectangle(0, 0, maxX + 1, row + 1);
            return map;
        }

    }
}