using JetBrains.Annotations;
using Console = SadConsole.Console;

namespace RogueEntity.SadCons
{
    public interface IConsoleContext: IConsoleParentContext
    {
        bool IsVisible { get; set; }
        void Initialize([NotNull] IConsoleParentContext parentContext);

        Console Console { get; }

        void Show();
        void Hide();
    }
}
