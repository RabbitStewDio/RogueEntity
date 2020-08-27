using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.Naming
{
    public static class DisplayNameExtensions
    {
        public static readonly IDisplayName Something = new NonCountableNounDisplayName("something");
        public static readonly IDisplayName Somebody = new NonCountableNounDisplayName("somebody");

        public static IDisplayName ToItemName<TGameContext, TItemId>(this TItemId actor,
                                                                     TGameContext context)
            where TGameContext : IItemContext<TGameContext, TItemId>
            where TItemId : IEntityKey
        {
            if (context.ItemResolver.TryQueryData(actor, context, out IDisplayName name))
            {
                return name;
            }

            return Something;
        }
/*
        public static IDisplayName ToActorName<TGameContext, TActorId>(this TActorId actor,
                                                                       TGameContext context)
            where TGameContext : IActorContext<TGameContext, TActorId>
            where TActorId : IEntityKey
        {
            if (context.ActorResolver.TryQueryData(actor, context, out IDisplayName name))
            {
                return name;
            }

            return Somebody;
        }

        public static string ToDefiniteActorName<TGameContext, TActorId>(this TActorId actor, TGameContext context, int count = 1)
            where TGameContext : IActorContext<TGameContext, TActorId>
            where TActorId : IEntityKey
        {
            if (context.ActorResolver.TryQueryData(actor, context, out IDisplayName name))
            {
                return name.GetDefiniteFormName(count);
            }

            return Somebody.GetDefiniteFormName(count);
        }
        public static string ToIndefiniteActorName<TGameContext, TActorId>(this TActorId actor, TGameContext context, int count = 1)
            where TGameContext : IActorContext<TGameContext, TActorId>
            where TActorId : IEntityKey
        {
            if (context.ActorResolver.TryQueryData(actor, context, out IDisplayName name))
            {
                return name.GetIndefiniteFormName(count);
            }

            return Somebody.GetIndefiniteFormName(count);
        }
*/

        public static string ToDefiniteItemName<TGameContext, TItemId>(this TItemId actor, TGameContext context, int count = 1)
            where TGameContext : IItemContext<TGameContext, TItemId>
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            if (context.ItemResolver.TryQueryData(actor, context, out IDisplayName name))
            {
                return name.GetDefiniteFormName(count);
            }

            return Something.GetDefiniteFormName(count);
        }

        public static string ToIndefiniteItemName<TGameContext, TItemId>(this TItemId actor, TGameContext context, int count = 1)
            where TGameContext : IItemContext<TGameContext, TItemId>
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            if (context.ItemResolver.TryQueryData(actor, context, out IDisplayName name))
            {
                return name.GetIndefiniteFormName(count);
            }

            return Something.GetIndefiniteFormName(count);
        }
    }
}