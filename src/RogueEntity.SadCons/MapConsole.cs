using EnTTSharp.Entities;
using RogueEntity.Api.Services;
using RogueEntity.Core.Players;
using SadConsole;

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
        }

        public void Initialize<TPlayerEntity>(IServiceResolver services)
            where TPlayerEntity : IEntityKey
        {
            this.backend.Components.Clear();
            this.backend.Components.Add(new MapConsoleMouseHandler<TPlayerEntity>(sharedState,
                                                                                  services.Resolve<IPlayerService<TPlayerEntity>>()));
        }
    }
}
