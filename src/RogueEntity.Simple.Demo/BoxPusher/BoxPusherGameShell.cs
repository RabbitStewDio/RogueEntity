using RogueEntity.Core.Players;
using RogueEntity.SadCons;
using RogueEntity.Simple.BoxPusher.ItemTraits;

namespace RogueEntity.Simple.Demo.BoxPusher
{
    public class BoxPusherGameShell : GameShell<BoxPusherPlayerProfile>
    {
        readonly BoxPusherGame game;
        readonly BoxPusherProfileItemTheme itemTheme;

        public BoxPusherGameShell()
        {
            game = new BoxPusherGame();
            itemTheme = new BoxPusherProfileItemTheme(3);
        }

        protected override void InitializeOverride()
        {
            game.InitializeSystems();
        }

        protected override void InitializeLateOverride()
        {
            base.InitializeLateOverride();
            LoadProfileContext.ListItemRenderer = itemTheme;
        }

        protected override IPlayerProfileManager<BoxPusherPlayerProfile> ProfileManager => game.ProfileManager;
    }
}
