using System;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Meta.ItemTraits
{
    public static class ItemResolverExtensions
    {
        public static StackCount QueryStackSize<TItemId>(this IItemResolver<TItemId> resolver,
                                                         TItemId item)
            where TItemId : IEntityKey
        {
            if (item.IsEmpty)
            {
                throw new ArgumentException();
            }

            if (resolver.TryQueryData(item, out StackCount p))
            {
                return p;
            }

            return StackCount.One;
        }

        public static WeightView QueryWeight<TItemId>(this IItemResolver<TItemId> resolver,
                                                      TItemId item)
            where TItemId : IEntityKey
        {
            if (resolver.TryQueryData(item, out WeightView p))
            {
                return p;
            }

            return default;
        }

        public static Weight QueryBaseWeight<TItemId>(this IItemResolver<TItemId> resolver,
                                                      TItemId r)
            where TItemId : IEntityKey
        {
            if (resolver.TryQueryData(r, out Weight p))
            {
                return p;
            }

            return default;
        }

        public static bool SplitLargeStack<TItemId>(this IItemResolver<TItemId> itemResolver,
                                                    TItemId r,
                                                    int count,
                                                    out TItemId taken,
                                                    out TItemId remainder,
                                                    out int remainingCount)
            where TItemId : IEntityKey
        {
            if (count < 0)
            {
                throw new ArgumentException();
            }

            if (count == 0)
            {
                taken = default;
                remainder = r;
                remainingCount = count;
                return false;
            }

            var s = itemResolver.QueryStackSize(r);
            var maxApplicable = Math.Min(count, s.MaximumStackSize);
            var guaranteedRemaining = count - maxApplicable;

            if (itemResolver.SplitStack(r, (ushort)maxApplicable, out taken, out remainder, out var remaining))
            {
                remainingCount = remaining + guaranteedRemaining;
                return true;
            }

            taken = default;
            remainder = r;
            remainingCount = count;
            return false;
        }

        public static bool SplitStack<TItemId>(this IItemResolver<TItemId> itemResolver,
                                               TItemId r,
                                               int count,
                                               out TItemId taken,
                                               out TItemId remainder,
                                               out int remainingCount)
            where TItemId : IEntityKey
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (count == 0)
            {
                // NoOp.
                taken = default;
                remainder = r;
                remainingCount = 0;
                return false;
            }

            if (!itemResolver.TryQueryData(r, out StackCount p))
            {
                // If not stackable assume a max-stack size of 1
                taken = r;
                remainder = default;
                remainingCount = (count - 1);
                return true;
            }

            if (count >= p.Count)
            {
                taken = r;
                remainder = default;
                remainingCount = (count - p.Count);
                return true;
            }

            // count < p.Count

            var takeResult = p.Take(count);
            if (takeResult.ItemsLeftInStack.TryGetValue(out var remainderStack))
            {
                if (itemResolver.TryUpdateData(r, in remainderStack, out remainder) &&
                    itemResolver.TryUpdateData(r, in takeResult.ItemsTakenFromStack, out taken))
                {
                    remainingCount = takeResult.ItemsNotAvailableInStack;
                    return true;
                }

                taken = default;
                remainder = r;
                remainingCount = count;
                return false;
            }

            taken = r;
            remainder = default;
            remainingCount = takeResult.ItemsNotAvailableInStack;
            return true;
        }
    }
}
