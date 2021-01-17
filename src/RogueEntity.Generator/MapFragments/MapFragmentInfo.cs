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
        public readonly MapFragmentConnectivity Connectivity;

        public MapFragmentInfo(string name, 
                               string type,
                               MapFragmentConnectivity connectivity, 
                               RuleProperties properties = default, 
                               ReadOnlyListWrapper<string> tags = default)
        {
            Name = name;
            Type = type;
            Properties = properties ?? new RuleProperties();
            Tags = tags;
            Connectivity = connectivity;
        }

        public Optional<string> TryQueryTagRestrictions(MapFragmentConnectivity c)
        {
            if (Connectivity.HasFlags(c))
            {
                if (Properties.TryGetValue("Require_" + c, out string tagPattern))
                {
                    return Optional.ValueOf(tagPattern);
                }

                return Optional.ValueOf("");
            }

            return Optional.Empty<string>();
        }
    }
}