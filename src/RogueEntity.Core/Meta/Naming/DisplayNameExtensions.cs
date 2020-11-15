﻿using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Meta.Naming
{
    public static class DisplayNameExtensions
    {
        public static readonly IDisplayName Something = new NonCountableNounDisplayName("something");
        public static readonly IDisplayName Somebody = new NonCountableNounDisplayName("somebody");

        public static IDisplayName ToItemName<TGameContext, TItemId>(this TItemId actor,
                                                                     IItemResolver<TGameContext, TItemId> itemResolver,
                                                                     TGameContext context)
            where TItemId : IEntityKey
        {
            if (itemResolver.TryQueryData(actor, context, out IDisplayName name))
            {
                return name;
            }

            return Something;
        }

        public static BulkItemDeclarationBuilder<TGameContext, TItemId> WithName<TGameContext, TItemId>(this BulkItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                 string name)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new DefaultItemNameTrait<TGameContext, TItemId>(new CountableNounDisplayName(name)));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithName<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                      string name)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new DefaultItemNameTrait<TGameContext, TItemId>(new CountableNounDisplayName(name)));
            return builder;
        }

        public static BulkItemDeclarationBuilder<TGameContext, TItemId> WithUniqueName<TGameContext, TItemId>(this BulkItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                              string name)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new DefaultItemNameTrait<TGameContext, TItemId>(new NonCountableNounDisplayName(name)));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithUniqueName<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                   string name)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new DefaultItemNameTrait<TGameContext, TItemId>(new NonCountableNounDisplayName(name)));
            return builder;
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

        public static string ToDefiniteItemName<TGameContext, TItemId>(this TItemId actor, 
                                                                       IItemResolver<TGameContext, TItemId> itemResolver,
                                                                       TGameContext context, 
                                                                       int count = 1)
            where TItemId : IEntityKey
        {
            if (itemResolver.TryQueryData(actor, context, out IDisplayName name))
            {
                return name.GetDefiniteFormName(count);
            }

            return Something.GetDefiniteFormName(count);
        }

        public static string ToIndefiniteItemName<TGameContext, TItemId>(this TItemId actor, 
                                                                         IItemResolver<TGameContext, TItemId> itemResolver,
                                                                         TGameContext context, 
                                                                         int count = 1)
            where TItemId : IEntityKey
        {
            if (itemResolver.TryQueryData(actor, context, out IDisplayName name))
            {
                return name.GetIndefiniteFormName(count);
            }

            return Something.GetIndefiniteFormName(count);
        }
    }
}