using RogueEntity.Core.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueEntity.Core.Players
{
    public class DefaultPlayerProfileManager<TProfileData> : IPlayerProfileManager<TProfileData>
    {
        readonly IDataRepository<Guid, TProfileData> repository;

        public DefaultPlayerProfileManager(IDataRepository<Guid, TProfileData> repository)
        {
            this.repository = repository;
        }

        public bool TryCreatePlayer(in TProfileData profile, out Guid playerId, out TProfileData profileData)
        {
            for (var attempt = 0; attempt < 100; attempt += 1)
            {
                playerId = Guid.NewGuid();
                if (!repository.TryStore(playerId, profile))
                {
                    continue;
                }

                profileData = profile;
                return true;
            }

            profileData = default;
            playerId = default;
            return false;
        }

        public IReadOnlyList<Guid> KnownPlayerIds => repository.QueryEntries().ToList();

        public bool TryDiscardPlayerState(Guid playerId)
        {
            return repository.TryDelete(playerId);
        }

        public bool TryLoadPlayerData(Guid playerId, out TProfileData profileData)
        {
            return repository.TryRead(playerId, out profileData);
        }
    }
}
