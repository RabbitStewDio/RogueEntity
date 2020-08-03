using System;
using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure.Meta.Items;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Infrastructure.Meta.ItemBuilder
{
    public abstract class ItemBuilderBase<TGameContext, TItemId, TBuilder>
        where TBuilder : ItemBuilderBase<TGameContext, TItemId, TBuilder>
        where TItemId : IEntityKey
    {
        protected readonly TGameContext Context;
        protected readonly IItemResolver<TGameContext, TItemId> ItemResolver;
        protected TItemId Reference;

        protected ItemBuilderBase(TGameContext context, IItemResolver<TGameContext, TItemId> itemResolver, TItemId reference)
        {
            this.Context = context;
            this.ItemResolver = itemResolver;
            this.Reference = reference;
        }

        public virtual TBuilder WithStackCount(ushort count)
        {
            if (ItemResolver.TryQueryData(Reference, Context, out StackCount stack))
            {
                ItemResolver.TryUpdateData(Reference, Context, stack.WithCount(count), out Reference);
            }

            return (TBuilder)this;
        }

        public virtual TBuilder WithChargeCount(ushort count)
        {
            if (ItemResolver.TryQueryData(Reference, Context, out ItemCharge stack))
            {
                ItemResolver.TryUpdateData(Reference, Context, stack.WithCount(count), out Reference);
            }

            return (TBuilder)this;
        }

        public virtual TBuilder WithDurability(ushort count)
        {
            if (ItemResolver.TryQueryData(Reference, Context, out Durability stack))
            {
                ItemResolver.TryUpdateData(Reference, Context, stack.WithHitPoints(count), out Reference);
            }

            return (TBuilder)this;
        }

        public TBuilder WithData<TData>(TData data)
        {
            ItemResolver.TryUpdateData(Reference, Context, data, out Reference);
            return (TBuilder)this;
        }

        public TBuilder WithData<TData>(Func<TData, TData> data)
        {
            if (ItemResolver.TryQueryData(Reference, Context, out TData stack))
            {
                ItemResolver.TryUpdateData(Reference, Context, data(stack), out Reference);
            }

            return (TBuilder)this;
        }

        public TBuilder WithData<TData>(Func<TItemId, TGameContext, TData, TData> data)
        {
            if (ItemResolver.TryQueryData(Reference, Context, out TData stack))
            {
                ItemResolver.TryUpdateData(Reference, Context, data(Reference, Context, stack), out Reference);
            }

            return (TBuilder)this;
        }

        public TBuilder WithRandomizedProperties(Func<double> randomGenerator)
        {
            if (ItemResolver.TryQueryData(Reference, Context, out StackCount stackSize))
            {
                var stackCount = randomGenerator.Next(1, stackSize.MaximumStackSize + 1);
                stackSize = stackSize.WithCount((ushort)stackCount);
                if (ItemResolver.TryUpdateData(Reference, Context, stackSize, out var changedItemRef))
                {
                    Reference = changedItemRef;
                }
            }

            if (ItemResolver.TryQueryData(Reference, Context, out ItemCharge charge))
            {
                var chargeCount = randomGenerator.Next(1, charge.MaximumCharge + 1);
                if (ItemResolver.TryUpdateData(Reference, Context, charge.WithCount((ushort)chargeCount), out var changedItemRef))
                {
                    Reference = changedItemRef;
                }
            }

            if (ItemResolver.TryQueryData(Reference, Context, out Durability durability))
            {
                var durabilityHp = randomGenerator.Next(1, durability.MaxHitPoints + 1);
                if (ItemResolver.TryUpdateData(Reference, Context, durability.WithHitPoints((ushort)durabilityHp), out var changedItemRef))
                {
                    Reference = changedItemRef;
                }
            }

            return (TBuilder)this;
        }

        public static implicit operator TItemId(ItemBuilderBase<TGameContext, TItemId, TBuilder> itemBuilder)
        {
            return itemBuilder.Reference;
        }

        public TItemId ToItemReference => Reference;
    }

    public static class ItemBuilderExtensions
    {
        public sealed class ItemBuilder<TGameContext, TItemId> : ItemBuilderBase<TGameContext, TItemId, ItemBuilder<TGameContext, TItemId>>
            where TItemId : IEntityKey
        {
            public ItemBuilder(TGameContext context, 
                               IItemResolver<TGameContext, TItemId> resolver,
                               TItemId reference) : base(context, resolver, reference)
            {
            }
        }

        public static ItemBuilder<TGameContext, TItemId> Build<TGameContext, TItemId>(this IItemResolver<TGameContext, TItemId> resolver,
                                                                                      TGameContext context,
                                                                                      ItemDeclarationId declaration)
            where TItemId : IEntityKey
        {
            if (resolver.ItemRegistry.TryGetItemById(declaration, out var itemDefinition))
            {
                var instantiated = resolver.Instantiate(context, itemDefinition);
                return new ItemBuilder<TGameContext, TItemId>(context, resolver, instantiated);
            }

            throw new ArgumentException("Invalid item declaration " + declaration);
        }
    }
}