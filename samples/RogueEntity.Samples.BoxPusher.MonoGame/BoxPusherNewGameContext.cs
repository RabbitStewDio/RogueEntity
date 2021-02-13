using RogueEntity.Core.Players;
using RogueEntity.SadCons;
using RogueEntity.Simple.BoxPusher.ItemTraits;
using SadConsole;

namespace RogueEntity.Simple.Demo.BoxPusher
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
