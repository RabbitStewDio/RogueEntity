using JetBrains.Annotations;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace RogueEntity.Generator.MapFragments
{
    /// <summary>
    ///   Temporary object using during parsing. This object is instantiated via
    ///   reflection and all setters are called via reflection. So shut up, Inspector.
    /// </summary>
    [UsedImplicitly]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    internal class MapFragmentParseInfo
    {
        public string Template { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public MapFragmentSize Size { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public Dictionary<char, List<string>> Symbols { get; set; }
        public List<string> Tags { get; set; }

        public static MapFragmentParseInfo Merge(MapFragmentParseInfo template, MapFragmentParseInfo dest)
        {
            var result = new MapFragmentParseInfo();
            result.Guid = dest.Guid;
            result.Name = dest.Name;
            result.Type = MergeStrings(template.Type, dest.Type);
            result.Size = MapFragmentSize.IsEmpty(dest.Size) ? template.Size : dest.Size;
            result.Properties = MergeProperties(template.Properties, dest.Properties);
            result.Symbols = MergeSymbols(template.Symbols, dest.Symbols);
            result.Tags = template.Tags.Concat(dest.Tags).ToList();
            return result;
        }

        static Dictionary<string, string> MergeProperties(Dictionary<string, string> template, Dictionary<string, string> right)
        {
            return template.Concat(right)
                           .Where(e => !string.IsNullOrWhiteSpace(e.Key) &&
                                       !string.IsNullOrWhiteSpace(e.Value))
                           .GroupBy(kvp => kvp.Key, kvp => kvp.Value)
                           .ToDictionary(g => g.Key, g => g.Last());
        }

        static Dictionary<char, List<string>> MergeSymbols(Dictionary<char, List<string>> template, Dictionary<char, List<string>> right)
        {
            return template.Concat(right)
                           .GroupBy(kvp => kvp.Key, kvp => kvp.Value)
                           .ToDictionary(g => g.Key, g => g.Last());
        }

        static string MergeStrings(string template, string dest)
        {
            if (string.IsNullOrWhiteSpace(dest))
            {
                return template;
            }

            return dest;
        }
    }
}
