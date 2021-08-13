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
            
            var size = ParentContext.Bounds.BoundsFor(Console.Font);
            Console.Position = new Point(size.Width - Console.Width - 2,
                                         size.Height - Console.Height - 2);

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
            var size = ParentContext.Bounds.BoundsFor(Console.Font);
            Console.Position = new Point(size.Width - Console.Width,
                                         size.Height - Console.Height);
            base.OnParentConsoleResized();
        }
    }
}
