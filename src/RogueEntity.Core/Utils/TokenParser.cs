using System;
using System.Collections.Generic;
using RogueEntity.Api.Utils;

namespace RogueEntity.Core.Utils
{
    public interface ITokenParser
    {
        bool TryParse<TData>(string text, out TData data);
    }

    public class TokenParser: ITokenParser
    {
        readonly Dictionary<Type, List<ITokenDefinition>> tokenDefinitions;

        public TokenParser()
        {
            this.tokenDefinitions = new Dictionary<Type, List<ITokenDefinition>>();
        }

        public void Add<TTargetType>(ITokenDefinition<TTargetType> tokenDef)
        {
            if (!this.tokenDefinitions.TryGetValue(typeof(TTargetType), out var tokenList))
            {
                tokenList = new List<ITokenDefinition>();
                this.tokenDefinitions[typeof(TTargetType)] = tokenList;
            }

            tokenList.Add(tokenDef);
            tokenList.Sort(WeightComparison);
        }

        public void AddToken<TTargetType>(string symbol, TTargetType result)
        {
            Add(new StaticTokenDefinition<TTargetType>(symbol, result));
        }

        public void AddNumericToken<TData>(TokenProducer<TData> prod)
        {
            Add(new NumericTokenDefinition<TData>(prod));
        }

        public void AddCompoundToken<TLeft, TRight, TData>(string separator, CompoundTokenProducer<TLeft, TRight, TData> prod)
        {
            Add(new CompoundTokenDefinition<TLeft, TRight, TData>(separator, prod));
        }

        int WeightComparison(ITokenDefinition x, ITokenDefinition y)
        {
            return x.Weight.CompareTo(y.Weight);
        }

        public bool TryParse<TData1>(string text, out TData1 data)
        {
            if (tokenDefinitions.TryGetValue(typeof(TData1), out var tokens))
            {
                foreach (var t in tokens)
                {
                    if (!(t is ITokenDefinition<TData1> d))
                    {
                        continue;
                    }

                    if (d.TryParse(text, this, out data))
                    {
                        return true;
                    }
                }
            }

            data = default;
            return false;
        }

        public interface ITokenDefinition
        {
            public int Weight { get; }
        }

        public interface ITokenDefinition<TData> : ITokenDefinition
        {
            public bool TryParse(string text, ITokenParser p, out TData data);
        }

        public delegate bool TokenProducer<TToken>(float data, out TToken result);

        public class NumericTokenDefinition<TData> : ITokenDefinition<TData>
        {
            public int Weight => 1;
            readonly TokenProducer<TData> producer;

            public NumericTokenDefinition(TokenProducer<TData> producer)
            {
                this.producer = producer;
            }

            public bool TryParse(string text, ITokenParser p, out TData data)
            {
                if (float.TryParse(text, out var rawData))
                {
                    return producer(rawData, out data);
                }

                data = default;
                return false;
            }
        }

        public delegate bool CompoundTokenProducer<TLeft, TRight, TToken>(in Optional<TLeft> left, in Optional<TRight> right, out TToken result);

        public class CompoundTokenDefinition<TLeft, TRight, TToken> : ITokenDefinition<TToken>
        {
            public int Weight => 2;
            readonly string separator;
            readonly CompoundTokenProducer<TLeft, TRight, TToken> producer;

            public CompoundTokenDefinition(string separator, CompoundTokenProducer<TLeft, TRight, TToken> producer)
            {
                this.separator = separator;
                this.producer = producer;
            }

            public bool TryParse(string text, ITokenParser p, out TToken data)
            {
                var idx = text.IndexOf(separator, StringComparison.Ordinal);
                if (idx == -1)
                {
                    if (typeof(TLeft) == typeof(TToken) ||
                        typeof(TRight) == typeof(TToken))
                    {
                        // make life simple, dont allow ambiguous recursive rules.
                        data = default;
                        return false;
                    }

                    if (p.TryParse(text, out TLeft left))
                    {
                        return producer(Optional.ValueOf(left), Optional.Empty(), out data);
                    }

                    if (p.TryParse(text, out TRight right))
                    {
                        return producer(Optional.Empty(), Optional.ValueOf(right), out data);
                    }
                }
                else
                {
                    var leftText = text.Substring(0, idx);
                    var rightText = text.Substring(idx + 1);
                    if (p.TryParse(leftText, out TLeft leftValue) &&
                        p.TryParse(rightText, out TRight rightValue))
                    {
                        return producer(Optional.ValueOf(leftValue), Optional.ValueOf(rightValue), out data);
                    }
                }

                data = default;
                return false;
            }
        }

        public static bool ParseFloat(float f, out float r)
        {
            r = f;
            return true;
        }
        
        public class StaticTokenDefinition<TData> : ITokenDefinition<TData>
        {
            readonly string symbol;
            readonly TData result;

            public StaticTokenDefinition(string symbol, TData result)
            {
                this.symbol = symbol;
                this.result = result;
            }

            public int Weight => 0;

            public bool TryParse(string text, ITokenParser p, out TData data)
            {
                if (string.Equals(symbol, text, StringComparison.Ordinal))
                {
                    data = result;
                    return true;
                }

                data = default;
                return false;
            }
        }
    }
}