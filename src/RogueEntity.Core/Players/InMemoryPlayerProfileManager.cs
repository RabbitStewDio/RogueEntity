using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace RogueEntity.Core.Players
{
    public class InMemoryPlayerProfileManager<TProfileData> : IPlayerProfileManager<TProfileData>
    {
        readonly Dictionary<Guid, TProfileData> data;

        public InMemoryPlayerProfileManager()
        {
            data = new Dictionary<Guid, TProfileData>();
        }

        public bool TryCreatePlayer(in TProfileData profile, out Guid playerId, [MaybeNullWhen(false)] out TProfileData profileData)
        {
            for (var attempt = 0; attempt < 100; attempt += 1)
            {
                playerId = Guid.NewGuid();
                if (data.ContainsKey(playerId))
                {
                    continue;
                }

                data[playerId] = profile;
                profileData = profile;
                return true;
            }

            profileData = default;
            playerId = default;
            return false;
        }

        public IReadOnlyList<Guid> KnownPlayerIds => data.Keys.ToList();
        
        public bool TryDiscardPlayerState(Guid playerId)
        {
            return data.Remove(playerId);
        }

        public bool TryLoadPlayerData(Guid playerId, out TProfileData profileData)
        {
            return data.TryGetValue(playerId, out profileData);
        }
    }
}
