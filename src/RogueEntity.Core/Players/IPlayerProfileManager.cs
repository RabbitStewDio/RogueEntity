using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Players
{
    public interface IPlayerProfileManager<TProfileData>
    {
        bool TryCreatePlayer(in TProfileData profile, out Guid id, [MaybeNullWhen(false)] out TProfileData profileData);
        
        IReadOnlyList<Guid> KnownPlayerIds { get; }
        
        bool TryDiscardPlayerState(Guid playerId);

        bool TryLoadPlayerData(Guid playerId, [MaybeNullWhen(false)] out TProfileData profileData);
    }
}
