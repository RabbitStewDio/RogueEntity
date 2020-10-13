using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RogueEntity.Core.Utils
{
    public static class CoreExtensions
    {
        public static int Next(this Func<double> g, int lowerBound, int upperBoundExclusive)
        {
            if (lowerBound >= upperBoundExclusive)
            {
                return lowerBound;
            }

            var delta = upperBoundExclusive - lowerBound;
            var floor = (int)(g() * delta);
            if (floor == delta)
            {
                // cheap hack. When using float the system happily 
                // rounds up.
                floor = delta - 1;
            }
            return floor + lowerBound;
        }

        public static float Clamp(this float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        public static int Clamp(this int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        public static ushort Clamp(this ushort value, ushort min, ushort max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        public static TEnum[] GetValues<TEnum>() where TEnum : struct, IComparable, IConvertible, IFormattable
        {
            return (TEnum[]) Enum.GetValues(typeof(TEnum));
        }

        public static TEnum[] GetValues<TEnum>(this TEnum v)
            where TEnum : struct, IComparable, IConvertible, IFormattable
        {
            return (TEnum[]) Enum.GetValues(typeof(TEnum));
        }

        public static bool HasFlags<TEnum>(this TEnum value, TEnum flag)
            where TEnum : struct, IComparable, IConvertible, IFormattable
        {
            return EnumExtensionsInternal<TEnum>.HasFlagsDelegate(value, flag);
        }

        static class EnumExtensionsInternal<TEnum> where TEnum : struct, IComparable, IConvertible, IFormattable
        {
            /// <summary>
            /// The delegate which determines if a flag is set.
            /// </summary>
            public static readonly Func<TEnum, TEnum, bool> HasFlagsDelegate = CreateHasFlagDelegate();

            /// <summary>
            /// Creates the has flag delegate.
            /// <code>
            /// (enum, flag) => {
            ///      uint64 tmp = (uint64) flag;
            ///      return ((uint64) value & tmp) == tmp;
            /// }
            /// </code>
            /// </summary>
            /// <returns></returns>
            static Func<TEnum, TEnum, bool> CreateHasFlagDelegate()
            {
                if (!typeof(TEnum).IsEnum)
                {
                    throw new ArgumentException(String.Format("{0} is not an Enum", typeof(TEnum)),
                                                typeof(EnumExtensionsInternal<>).GetGenericArguments()[0].Name);
                }

                var valueExpression = Expression.Parameter(typeof(TEnum));
                var flagExpression = Expression.Parameter(typeof(TEnum));
                var flagValueVariable = Expression.Variable(Type.GetTypeCode(typeof(TEnum)) == TypeCode.UInt64
                                                                ? typeof(ulong)
                                                                : typeof(long));
                var lambdaExpression = Expression.Lambda<Func<TEnum, TEnum, bool>>(
                    Expression.Block(
                        new[] {flagValueVariable},
                        Expression.Assign(flagValueVariable,
                                          Expression.Convert(flagExpression, flagValueVariable.Type)),
                        Expression.Equal(
                            Expression.And(Expression.Convert(valueExpression, flagValueVariable.Type),
                                           flagValueVariable), flagValueVariable)
                    ),
                    valueExpression,
                    flagExpression
                );
                return lambdaExpression.Compile();
            }
        }

        public static bool EqualsList<TItem>(ReadOnlyListWrapper<TItem> a, ReadOnlyListWrapper<TItem> b)
        {
            if (a.Count != b.Count) return false;
            
            for (var i = 0; i < a.Count; i++)
            {
                if (!EqualityComparer<TItem>.Default.Equals(a[i], b[i]))
                {
                    return false;
                }
            }

            return true;
        }
        
        public static bool EqualsList<TItem>(IReadOnlyList<TItem> a, IReadOnlyList<TItem> b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            if (a.Count != b.Count) return false;
            
            for (var i = 0; i < a.Count; i++)
            {
                if (!EqualityComparer<TItem>.Default.Equals(a[i], b[i]))
                {
                    return false;
                }
            }

            return true;
        }
        
        public static bool EqualsDictionary<TKey, TItem>(Dictionary<TKey, TItem> a, Dictionary<TKey, TItem> b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            if (a.Count != b.Count) return false;
            
            foreach (var entry in a)
            {
                if (!b.TryGetValue(entry.Key, out var otherValue) ||
                    !EqualityComparer<TItem>.Default.Equals(entry.Value, otherValue))
                {
                    return false;
                }
            }

            return true;
        }
    }
}