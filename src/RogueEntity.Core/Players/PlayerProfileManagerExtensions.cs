using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace RogueEntity.Core.Players
{
    public static class PlayerProfileManagerExtensions
    {
        public static IEnumerable<TPlayerProfile> ReadProfiles<TPlayerProfile>(this IPlayerProfileManager<TPlayerProfile> p)
        {
            var knownPlayerIds = p.KnownPlayerIds.ToArray();
            foreach (var guid in knownPlayerIds)
            {
                if (p.TryLoadPlayerData(guid, out var profile))
                {
                    yield return profile;
                }
            }
        } 
    }
}
