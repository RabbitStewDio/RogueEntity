using Microsoft.Xna.Framework.Input;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using Keyboard = SadConsole.Input.Keyboard;

namespace RogueEntity.SadCons.Util
{
    public class SadConsoleKeyboard: IKeyboard<Keys>
    {
        readonly Keyboard keyboard;

        public SadConsoleKeyboard(Keyboard keyboard)
        {
            this.keyboard = keyboard;
        }

        public bool IsKeyPressed(Keys k)
        {
            return keyboard.IsKeyPressed(k);
        }

        public bool IsNumericKeyPressed(out int number)
        {
            if (keyboard.IsKeyPressed(Keys.D0))
            {
                number = 0;
                return true;
            }
            if (keyboard.IsKeyPressed(Keys.D1))
            {
                number = 1;
                return true;
            }
            if (keyboard.IsKeyPressed(Keys.D2))
            {
                number = 2;
                return true;
            }
            if (keyboard.IsKeyPressed(Keys.D3))
            {
                number = 3;
                return true;
            }
            if (keyboard.IsKeyPressed(Keys.D4))
            {
                number = 4;
                return true;
            }
            if (keyboard.IsKeyPressed(Keys.D5))
            {
                number = 5;
                return true;
            }
            if (keyboard.IsKeyPressed(Keys.D6))
            {
                number = 6;
                return true;
            }
            if (keyboard.IsKeyPressed(Keys.D7))
            {
                number = 7;
                return true;
            }
            if (keyboard.IsKeyPressed(Keys.D8))
            {
                number = 8;
                return true;
            }
            if (keyboard.IsKeyPressed(Keys.D9))
            {
                number = 9;
                return true;
            }

            number = -1;
            return false;
        }

        public bool IsDirectionKeyPressed(out Direction d)
        {
            var info = keyboard;
            if (info.IsKeyPressed(Keys.Up) ||
                info.IsKeyPressed(Keys.NumPad8))
            {
                d = Direction.Up;
                return true;
            }

            if (info.IsKeyPressed(Keys.Down) ||
                info.IsKeyPressed(Keys.NumPad2))
            {
                d = Direction.Down;
                return true;
            }

            if (info.IsKeyPressed(Keys.Left) ||
                info.IsKeyPressed(Keys.NumPad4))
            {
                d = Direction.Left;
                return true;
            }

            if (info.IsKeyPressed(Keys.Right) ||
                info.IsKeyPressed(Keys.NumPad6))
            {
                d = Direction.Right;
                return true;
            }

            if (info.IsKeyPressed(Keys.NumPad7))
            {
                d = Direction.UpLeft;
                return true;
            }

            if (info.IsKeyPressed(Keys.NumPad9))
            {
                d = Direction.UpRight;
                return true;
            }

            if (info.IsKeyPressed(Keys.NumPad3))
            {
                d = Direction.DownRight;
                return true;
            }

            if (info.IsKeyPressed(Keys.NumPad1))
            {
                d = Direction.DownLeft;
                return true;
            }

            d = Direction.None;
            return false;
        }

        public bool IsBackspaceKeyPressed()
        {
            return keyboard.IsKeyPressed(Keys.Back);
        }

        public bool IsConfirmKeyPressed()
        {
            return keyboard.IsKeyPressed(Keys.Enter);
        }

        public bool IsCancelPressed()
        {
            return keyboard.IsKeyPressed(Keys.Escape);
        }

        public bool IsModifierDown(ModifierKeys m)
        {
            if (m.HasFlags(ModifierKeys.Shift) && !keyboard.IsKeyDown(Keys.LeftShift) && !keyboard.IsKeyDown(Keys.RightShift))
            {
                return false;
            }
            if (m.HasFlags(ModifierKeys.Alt) && !keyboard.IsKeyDown(Keys.LeftAlt) && !keyboard.IsKeyDown(Keys.RightAlt))
            {
                return false;
            }
            if (m.HasFlags(ModifierKeys.Control) && !keyboard.IsKeyDown(Keys.LeftControl) && !keyboard.IsKeyDown(Keys.RightControl))
            {
                return false;
            }

            return true;
        }

        public bool IsAllModifiersDown(ReadOnlyListWrapper<ModifierKeys> modifiers)
        {
            foreach (var m in modifiers)
            {
                if (!IsModifierDown(m))
                    return false;
            }

            return true;
        }
    }
}