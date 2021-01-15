using System;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.Meta.ItemTraits;

namespace RogueEntity.Core.Meta.ItemBuilder
{
    public abstract class ItemBuilderBase<TItemId, TBuilder>
        where TBuilder : ItemBuilderBase<TItemId, TBuilder>
        where TItemId : IEntityKey
    {
        protected readonly IItemResolver<TItemId> ItemResolver;
        protected TItemId Reference;

        protected ItemBuilderBase(IItemResolver<TItemId> itemResolver, TItemId reference)
        {
            this.ItemResolver = itemResolver;
            this.Reference = reference;
        }

        public virtual TBuilder WithStackCount(ushort count)
        {
            if (ItemResolver.TryQueryData(Reference, out StackCount stack))
            {
                ItemResolver.TryUpdateData(Reference, stack.WithCount(count), out Reference);
            }

            return (TBuilder)this;
        }

        public virtual TBuilder WithChargeCount(ushort count)
        {
            if (ItemResolver.TryQueryData(Reference, out ItemCharge stack))
            {
                ItemResolver.TryUpdateData(Reference, stack.WithCount(count), out Reference);
            }

            return (TBuilder)this;
        }

        public virtual TBuilder WithDurability(ushort count)
        {
            if (ItemResolver.TryQueryData(Reference, out Durability stack))
            {
                ItemResolver.TryUpdateData(Reference, stack.WithHitPoints(count), out Reference);
            }

            return (TBuilder)this;
        }

        public TBuilder WithData<TData>(TData data)
        {
            ItemResolver.TryUpdateData(Reference, data, out Reference);
            return (TBuilder)this;
        }

        public TBuilder WithData<TData>(Func<TData, TData> data)
        {
            if (ItemResolver.TryQueryData(Reference, out TData stack))
            {
                ItemResolver.TryUpdateData(Reference,  data(stack), out Reference);
            }

            return (TBuilder)this;
        }

        public TBuilder WithData<TData>(Func<TItemId,  TData, TData> data)
        {
            if (ItemResolver.TryQueryData(Reference,  out TData stack))
            {
                ItemResolver.TryUpdateData(Reference,  data(Reference,  stack), out Reference);
            }

            return (TBuilder)this;
        }

        public TBuilder WithRandomizedProperties(IRandomGenerator randomGenerator)
        {
            if (ItemResolver.TryQueryData(Reference,  out StackCount stackSize))
            {
                var stackCount = randomGenerator.Next(1, stackSize.MaximumStackSize + 1);
                stackSize = stackSize.WithCount((ushort)stackCount);
                if (ItemResolver.TryUpdateData(Reference,  stackSize, out var changedItemRef))
                {
                    Reference = changedItemRef;
                }
            }

            if (ItemResolver.TryQueryData(Reference,  out ItemCharge charge))
            {
                var chargeCount = randomGenerator.Next(1, charge.MaximumCharge + 1);
                if (ItemResolver.TryUpdateData(Reference,  charge.WithCount((ushort)chargeCount), out var changedItemRef))
                {
                    Reference = changedItemRef;
                }
            }

            if (ItemResolver.TryQueryData(Reference,  out Durability durability))
            {
                var durabilityHp = randomGenerator.Next(1, durability.MaxHitPoints + 1);
                if (ItemResolver.TryUpdateData(Reference,  durability.WithHitPoints((ushort)durabilityHp), out var changedItemRef))
                {
                    Reference = changedItemRef;
                }
            }

            return (TBuilder)this;
        }

        public static implicit operator TItemId(ItemBuilderBase< TItemId, TBuilder> itemBuilder)
        {
            return itemBuilder.Reference;
        }

        public TItemId ToItemReference => Reference;
    }
}
