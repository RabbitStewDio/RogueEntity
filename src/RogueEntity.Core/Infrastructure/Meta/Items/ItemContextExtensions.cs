using EnttSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Meta.Items
{
    public static class ItemContextExtensions
    {
        // public static StackCount QueryStackSize<TGameContext, TItemId>(this TGameContext context, TItemId r)
        //     where TGameContext : IItemContext<TGameContext, TItemId> 
        //     where TItemId : IEntityKey
        // {
        //     if (context.ItemResolver.TryQueryData(r, context, out StackCount p))
        //     {
        //         return p;
        //     }
        //
        //     return StackCount.Of(1).WithCount(1);
        // }
        //
        // public static ItemCharge QueryCharges<TGameContext, TItemId>(this TGameContext context, TItemId r)
        //     where TGameContext : IItemContext<TGameContext, TItemId> 
        //     where TItemId : IEntityKey
        // {
        //     if (context.ItemResolver.TryQueryData(r, context, out ItemCharge p))
        //     {
        //         return p;
        //     }
        //
        //     return new ItemCharge(ushort.MaxValue, ushort.MaxValue);
        // }
        //
        // public static Weight QueryBaseWeight<TGameContext, TItemId>(this TGameContext context, TItemId r)
        //     where TGameContext : IItemContext<TGameContext, TItemId>
        //     where TItemId : IEntityKey
        // {
        //     if (context.ItemResolver.TryQueryData(r, context, out Weight p))
        //     {
        //         return p;
        //     }
        //
        //     return default;
        // }
    }
}