using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Players
{
    public interface IPlayerProfileManager<TProfileData>
    {
        bool TryCreatePlayer(in TProfileData profile, out Guid id, out TProfileData profileData);
        
        IReadOnlyList<Guid> KnownPlayerIds { get; }
        
        bool TryDiscardPlayerState(Guid playerId);

        bool TryLoadPlayerData(Guid playerId, out TProfileData profileData);
    }
}
