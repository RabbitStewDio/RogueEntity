using RogueEntity.Api.Utils;
using RogueEntity.Core.Utils;

namespace RogueEntity.Generator.MapFragments
{
    public readonly struct MapFragmentInfo
    {
        public readonly string Name;
        public readonly string Type;
        public readonly RuleProperties Properties;
        public readonly ReadOnlyListWrapper<string> Tags;

        public MapFragmentInfo(string name, 
                               string type,
                               RuleProperties properties = default, 
                               ReadOnlyListWrapper<string> tags = default)
        {
            Name = name;
            Type = type;
            Properties = properties ?? new RuleProperties();
            Tags = tags;
        }

        public MapFragmentInfo WithName(string name)
        {
            return new MapFragmentInfo(name, Type, Properties, Tags);
        }
    }
}