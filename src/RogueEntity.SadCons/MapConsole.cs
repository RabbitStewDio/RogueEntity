using EnTTSharp.Entities;
using RogueEntity.Core.Players;
using SadConsole;
using System;
using Console = SadConsole.Console;

namespace RogueEntity.SadCons
{
    public class MapConsole
    {
        readonly Console backend;
        readonly MapConsoleState sharedState;

        public MapConsole(Console backend)
        {
            this.backend = backend;
            this.sharedState = new MapConsoleState();
            this.sharedState.PlayerId = new PlayerTag(PlayerIds.SinglePlayer);
        }

        public PlayerTag PlayerId
        {
            get => sharedState.PlayerId;
            set => sharedState.PlayerId = value;
        }

        public void Initialize<TPlayerEntity>(IPlayerService<TPlayerEntity> playerService)
            where TPlayerEntity : IEntityKey
        {
            this.backend.Components.RemoveAll();
            this.backend.Components.Add(new MapConsoleMouseHandler<TPlayerEntity>(sharedState, playerService));
            this.backend.Components.Add(new MapConsoleKeyboardHandler<TPlayerEntity>(sharedState, playerService));
        }
    }
}
