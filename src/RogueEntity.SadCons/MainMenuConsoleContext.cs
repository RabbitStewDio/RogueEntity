using Microsoft.Xna.Framework;
using SadConsole;
using System;

namespace RogueEntity.SadCons
{
    public class MainMenuConsoleContext : ConsoleContext<ControlsConsole>
    {
        public event Action OnNewGame;
        public event Action OnLoadGame;
        public event Action OnSettings;
        public event Action OnQuitGame;

        public bool HasLoadScreen;
        public bool HasSettings;

        public override void Initialize(IConsoleParentContext parentContext)
        {
            base.Initialize(parentContext);

            Console = new ControlsConsole(24, 19);
            Console.Position = new Point(this.ParentContext.Bounds.Width - Console.Width - 2,
                                         this.ParentContext.Bounds.Height - Console.Height - 2);

            int x = 2;
            int y = 2;
            Console.Add(SadConsoleControls.CreateButton("New Game", 20, 3)
                                          .WithAction(() => OnNewGame?.Invoke())
                                          .WithVerticalPlacementAt(x, ref y));

            if (HasLoadScreen)
            {
                Console.Add(SadConsoleControls.CreateButton("Load Game", 20, 3)
                                              .WithAction(() => OnLoadGame?.Invoke())
                                              .WithVerticalPlacementAt(x, ref y, 2));
            }

            if (HasSettings)
            {
                Console.Add(SadConsoleControls.CreateButton("Settings", 20, 3)
                                              .WithAction(() => OnSettings?.Invoke())
                                              .WithVerticalPlacementAt(x, ref y, 2));
            }

            Console.Add(SadConsoleControls.CreateButton("Quit", 20, 3)
                                          .WithAction(() => OnQuitGame?.Invoke())
                                          .WithVerticalPlacementAt(x, ref y, 2));
        }

        protected override void OnParentConsoleResized()
        {
            Console.Position = new Point(this.ParentContext.Bounds.Width - Console.Width,
                                         this.ParentContext.Bounds.Height - Console.Height);
            base.OnParentConsoleResized();
        }
    }
}
