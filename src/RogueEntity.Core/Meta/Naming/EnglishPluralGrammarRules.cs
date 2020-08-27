using System.Globalization;

namespace RogueEntity.Core.Meta.Naming
{
    public static class EnglishPluralGrammarRules
    {
        public static readonly CultureInfo English = new CultureInfo(0x0809); // The Queen's English.

        public static string NumberToString(this int number)
        {
            if (number < 0 || number > 12)
            {
                return number.ToString();
            }

            switch (number)
            {
                case 0: return "no";
                case 1: return "one";
                case 2: return "two";
                case 3: return "three";
                case 4: return "four";
                case 5: return "five";
                case 6: return "six";
                case 7: return "seven";
                case 8: return "eight";
                case 9: return "nine";
                case 10: return "ten";
                case 11: return "eleven";
                case 12: return "twelve";
            }

            return number.ToString();
        }

        public static bool IsVowel(this char c)
        {
            switch (c)
            {
                case 'a': return true;
                case 'e': return true;
                case 'i': return true;
                case 'o': return true;
                case 'u': return true;
                default: return false;
            }
        }

        public static string ReplaceSuffix(this string noun, string suffix, string replacement)
        {
            if (noun.EndsWith(suffix) && noun.Length > suffix.Length)
            {
                return noun.Substring(0, noun.Length - 2) + replacement;
            }

            return noun;
        }
    }
}