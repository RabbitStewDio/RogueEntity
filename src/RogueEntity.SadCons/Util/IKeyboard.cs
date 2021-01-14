using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.SadCons.Util
{
    public interface IKeyboard<in TKeyEnum>
    {
        bool IsKeyPressed(TKeyEnum k);
        bool IsNumericKeyPressed(out int number);
        bool IsDirectionKeyPressed(out Direction d);
        bool IsBackspaceKeyPressed();
        bool IsConfirmKeyPressed();
        bool IsCancelPressed();

        bool IsModifierDown(ModifierKeys m);
        bool IsAllModifiersDown(ReadOnlyListWrapper<ModifierKeys> modifiers);
    }

}
