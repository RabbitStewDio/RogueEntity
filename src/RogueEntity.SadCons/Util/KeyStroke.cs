using System;
using System.Collections.Generic;

namespace RogueEntity.SadCons.Util
{
    public readonly struct KeyStroke<TKeyEnum> : IEquatable<KeyStroke<TKeyEnum>>
        where TKeyEnum : IEquatable<TKeyEnum>
    {
        static readonly IEqualityComparer<TKeyEnum> KeyEquality = EqualityComparer<TKeyEnum>.Default;

        public KeyStroke(TKeyEnum key, ModifierKeys modifiers = ModifierKeys.None)
        {
            Modifiers = modifiers;
            Key = key;
        }

        public bool Equals(KeyStroke<TKeyEnum> other)
        {
            return Modifiers == other.Modifiers && EqualityComparer<TKeyEnum>.Default.Equals(Key, other.Key);
        }

        public override bool Equals(object obj)
        {
            return obj is KeyStroke<TKeyEnum> other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Modifiers * 397) ^ EqualityComparer<TKeyEnum>.Default.GetHashCode(Key);
            }
        }

        public static bool operator ==(KeyStroke<TKeyEnum> left, KeyStroke<TKeyEnum> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(KeyStroke<TKeyEnum> left, KeyStroke<TKeyEnum> right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        ///   Checks whether this Keystroke is a match for the other keystroke given.
        ///   The directionality of the comparison is important. It is assumed that the
        ///   keystroke given in "other" is the registered Keystroke of an Action.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsMatchForAction(KeyStroke<TKeyEnum> other)
        {
            return KeyEquality.Equals(Key, other.Key) && (Modifiers & other.Modifiers) == other.Modifiers;
        }

        public ModifierKeys Modifiers { get; }

        public TKeyEnum Key { get; }
    }
}
