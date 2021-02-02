using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Controls;

namespace RogueEntity.SadCons
{
    public class LoadGameContext : ConsoleContext<Window>
    {
        Button backButton;

        public override void Initialize(IConsoleContext parentContext)
        {
            base.Initialize(parentContext);
            Console = new Window(ParentContext.Bounds.Width - 10, ParentContext.Bounds.Height - 10)
            {
                Position = new Point(5, 5)
            };

            backButton = new Button(10, 1)
            {
                Text = "Back", 
                Position = new Point(Console.Width - 13, Console.Height - 3)
            };
            
            backButton.Click += (e, args) => OnBack();

            Console.Add(backButton);
            // Console.FillWithRandomGarbage();
        }

        void OnBack()
        {
            Hide();
        }


        protected override void OnParentConsoleResized()
        {

            var width = ParentContext.Bounds.Width - 10;
            var height = ParentContext.Bounds.Height - 10;
            Console.Resize(width, height, true, new Rectangle(0, 0, width, height));
            // Console.Resize(width, height, true);
            
            System.Console.WriteLine("Size: " + width + ", " + height);
            System.Console.WriteLine("ViewPort: " + Console.ViewPort);
            System.Console.WriteLine("ParentBounds: " + ParentContext.Bounds);
            // Console.SetRenderCells();

            backButton.Position = new Point(Console.Width - 13, Console.Height - 3);

            Console.IsDirty = true;
            FireConsoleResized();
        }

        public override void Show()
        {
            base.Show();
            Console.Show(true);
        }

        public override void Hide()
        {
            Console.Hide();
            base.Hide();
        }
    }
}
