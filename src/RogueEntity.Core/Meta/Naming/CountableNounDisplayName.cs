using EnTTSharp;
using System;

namespace RogueEntity.Core.Meta.Naming
{
    public class CountableNounDisplayName : IDisplayName
    {
        readonly string singular;
        readonly bool vowelSound;
        readonly string plural;

        public CountableNounDisplayName(string singular,
                                        string? plural = default,
                                        Optional<bool> vowelSound = default)
        {
            if (string.IsNullOrWhiteSpace(singular))
            {
                throw new ArgumentException();
            }

            this.singular = singular;
            if (!vowelSound.TryGetValue(out this.vowelSound))
            {
                this.vowelSound = singular[0].IsVowel();
            }

            this.plural = plural ?? ComputePlural(singular);
        }

        public string GetIndefiniteFormName(int number)
        {
            if (number == 1)
            {
                if (vowelSound)
                {
                    return $"an {singular}";
                }

                return $"a {singular}";
            }

            if (number == 0)
            {
                return $"{number.NumberToString()} {singular}";
            }

            return $"{number.NumberToString()} {plural}";
        }

        public string GetDefiniteFormName(int number)
        {
            if (number == 0)
            {
                return $"{EnglishPluralGrammarRules.NumberToString(number)} {singular}";
            }

            if (number == 1)
            {
                return $"the {singular}";
            }

            return $"the {EnglishPluralGrammarRules.NumberToString(number)} {plural}";
        }


        /// <summary>
        ///  See https://www.grammarly.com/blog/plural-nouns/
        /// </summary>
        /// <param name="singular"></param>
        /// <returns></returns>
        static string ComputePlural(string singular)
        {
            if (singular.EndsWith("on", StringComparison.InvariantCulture))
            {
                return singular.ReplaceSuffix("on", "a");
            }

            if (singular.EndsWith("is", StringComparison.InvariantCulture))
            {
                return singular.ReplaceSuffix("is", "es");
            }

            if (singular.EndsWith("us", StringComparison.InvariantCulture))
            {
                return singular.ReplaceSuffix("us", "i");
            }

            // Can have exceptions!
            if (singular.EndsWith("o", StringComparison.InvariantCulture))
            {
                return singular + "es";
            }

            if (singular.Length > 2 && singular.EndsWith("y", StringComparison.InvariantCulture))
            {
                if (singular[singular.Length - 2].IsVowel())
                {
                    return singular + "s";
                }

                return singular.ReplaceSuffix("y", "ies");
            }

            if (singular.EndsWith("f", StringComparison.InvariantCulture))
            {
                return singular.ReplaceSuffix("f", "ve");
            }

            if (singular.EndsWith("fe", StringComparison.InvariantCulture))
            {
                return singular.ReplaceSuffix("fe", "ve");
            }

            if (singular.EndsWith("s", StringComparison.InvariantCulture) ||
                singular.EndsWith("ss", StringComparison.InvariantCulture) ||
                singular.EndsWith("sh", StringComparison.InvariantCulture) ||
                singular.EndsWith("ch", StringComparison.InvariantCulture) ||
                singular.EndsWith("x", StringComparison.InvariantCulture) ||
                singular.EndsWith("z", StringComparison.InvariantCulture))
            {
                return singular + "es";
            }

            return singular + "s";
        }
    }
}