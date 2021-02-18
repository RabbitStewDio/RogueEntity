using RogueEntity.Core.Players;
using RogueEntity.SadCons;
using RogueEntity.Samples.BoxPusher.Core.ItemTraits;
using SadConsole;

namespace RogueEntity.Samples.BoxPusher.MonoGame
{
    public class BoxPusherNewGameContext: ConsoleContext<Window>
    {
        readonly IPlayerProfileManager<BoxPusherPlayerProfile> profileManager;

        public BoxPusherNewGameContext(IPlayerProfileManager<BoxPusherPlayerProfile> profileManager)
        {
            this.profileManager = profileManager;
        }
    }
}
