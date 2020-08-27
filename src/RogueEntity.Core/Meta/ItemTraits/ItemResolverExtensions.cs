using System;
using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.ItemTraits
{
    public static class ItemResolverExtensions
    {
        public static bool IsSameBulkDataType<TGameContext, TItemId>(this IItemResolver<TGameContext, TItemId> resolver,
                                                                     TItemId item, TItemId item2)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            if (!item.IsReference && !item2.IsReference)
            {
                return item.BulkItemId == item2.BulkItemId;
            }

            if (resolver.TryResolve(in item, out var itemDeclA) &&
                resolver.TryResolve(in item2, out var itemDeclB))
            {
                return itemDeclA.Id == itemDeclB.Id;
            }

            return false;
        }

        public static StackCount QueryStackSize<TGameContext, TItemId>(this IItemResolver<TGameContext, TItemId> resolver,
                                                                       TItemId item, TGameContext context)
            where TItemId : IEntityKey
        {
            if (resolver.TryQueryData(item, context, out StackCount p))
            {
                return p;
            }

            return StackCount.One;
        }

        public static WeightView QueryWeight<TGameContext, TItemId>(this IItemResolver<TGameContext, TItemId> resolver,
                                                                    TItemId item, TGameContext context)
            where TItemId : IEntityKey
        {
            if (resolver.TryQueryData(item, context, out WeightView p))
            {
                return p;
            }

            return default;
        }

        public static Weight QueryBaseWeight<TGameContext, TItemId>(this IItemResolver<TGameContext, TItemId> resolver, 
                                                                    TItemId r,
                                                                    TGameContext context) 
            where TItemId : IEntityKey
        {
            if (resolver.TryQueryData(r, context, out Weight p))
            {
                return p;
            }

            return default;
        }

        public static bool SplitLargeStack<TGameContext, TItemId>(this IItemResolver<TGameContext, TItemId> itemResolver,
                                                                  TGameContext context,
                                                                  TItemId r, int count,
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

            var s = itemResolver.QueryStackSize(r, context);
            var maxApplicable = Math.Min(count, s.MaximumStackSize);
            var guaranteedRemaining = count - maxApplicable;

            if (itemResolver.SplitStack(context, r, (ushort)maxApplicable, out taken, out remainder, out var remaining))
            {
                remainingCount = remaining + guaranteedRemaining;
                return true;
            }

            taken = default;
            remainder = r;
            remainingCount = count;
            return false;
        }

        public static bool SplitStack<TGameContext, TItemId>(this IItemResolver<TGameContext, TItemId> itemResolver,
                                                             TGameContext context,
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

            if (!itemResolver.TryQueryData(r, context, out StackCount p))
            {
                // If not stackable assume a max-stack size of 1
                taken = r;
                remainder = default;
                remainingCount = (ushort)(count - 1);
                return true;
            }

            if (count >= p.Count)
            {
                taken = r;
                remainder = default;
                remainingCount = (ushort)(count - p.Count);
                return true;
            }

            // count < p.Count

            var takenStack = p.Take(count, out var remainderStack, out var notApplied);
            if (itemResolver.TryUpdateData(r, context, in remainderStack, out remainder) &&
                itemResolver.TryUpdateData(r, context, in takenStack, out taken))
            {
                remainingCount = (ushort)notApplied;
                return true;
            }

            taken = default;
            remainder = r;
            remainingCount = count;
            return false;
        }
    }
}