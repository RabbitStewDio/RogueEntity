using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Meta.Naming
{
    public static class DisplayNameExtensions
    {
        public static readonly IDisplayName Something = new NonCountableNounDisplayName("something");
        public static readonly IDisplayName Somebody = new NonCountableNounDisplayName("somebody");

        public static IDisplayName ToItemName<TItemId>(this TItemId actor,
                                                       IItemResolver<TItemId> itemResolver)
            where TItemId : struct, IEntityKey
        {
            if (itemResolver.TryQueryData<IDisplayName>(actor, out var name))
            {
                return name;
            }

            return Something;
        }

        public static BulkItemDeclarationBuilder<TItemId> WithName<TItemId>(this BulkItemDeclarationBuilder<TItemId> builder,
                                                                            string name)
            where TItemId : struct, IEntityKey
        {
            builder.Declaration.WithTrait(new DefaultItemNameTrait<TItemId>(new CountableNounDisplayName(name)));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TItemId> WithName<TItemId>(this ReferenceItemDeclarationBuilder<TItemId> builder,
                                                                                 string name)
            where TItemId : struct, IEntityKey
        {
            builder.Declaration.WithTrait(new DefaultItemNameTrait<TItemId>(new CountableNounDisplayName(name)));
            return builder;
        }

        public static BulkItemDeclarationBuilder<TItemId> WithUniqueName<TItemId>(this BulkItemDeclarationBuilder<TItemId> builder,
                                                                                  string name)
            where TItemId : struct, IEntityKey
        {
            builder.Declaration.WithTrait(new DefaultItemNameTrait<TItemId>(new NonCountableNounDisplayName(name)));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TItemId> WithUniqueName<TItemId>(this ReferenceItemDeclarationBuilder<TItemId> builder,
                                                                                       string name)
            where TItemId : struct, IEntityKey
        {
            builder.Declaration.WithTrait(new DefaultItemNameTrait<TItemId>(new NonCountableNounDisplayName(name)));
            return builder;
        }

        public static string ToDefiniteItemName<TItemId>(this TItemId actor,
                                                         IItemResolver<TItemId> itemResolver,
                                                         int count = 1)
            where TItemId : struct, IEntityKey
        {
            if (itemResolver.TryQueryData<IDisplayName>(actor, out var name))
            {
                return name.GetDefiniteFormName(count);
            }

            return Something.GetDefiniteFormName(count);
        }

        public static string ToIndefiniteItemName<TItemId>(this TItemId actor,
                                                           IItemResolver<TItemId> itemResolver,
                                                           int count = 1)
            where TItemId : struct, IEntityKey
        {
            if (itemResolver.TryQueryData<IDisplayName>(actor, out var name))
            {
                return name.GetIndefiniteFormName(count);
            }

            return Something.GetIndefiniteFormName(count);
        }
    }
}
