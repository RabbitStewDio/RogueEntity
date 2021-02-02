using RogueEntity.SadCons;
using SadConsole;

namespace RogueEntity.Simple.Demo.BoxPusher
{
    public class BoxPusherGameShell : GameShell
    {
        BoxPusherGame game;

        public BoxPusherGameShell()
        {
            game = new BoxPusherGame();
        }

        protected override void InitializeOverride()
        {
            game.InitializeSystems();
        }

    }

    
    public class LoadGameScreen
    {
        
    }
}
