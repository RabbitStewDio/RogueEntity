using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Simple.Demo.BoxPusher
{
    public static class StandardItems
    {

        public static void DoSomething<TGameContext>(ItemDeclarationBuilder<TGameContext> t)
        {
            var itemDeclarationBuilderWithReferenceContext = t.ForEntity<ItemReference>();
            itemDeclarationBuilderWithReferenceContext.DefineWall();
        }

        static ReferenceItemDeclarationBuilder<TGameContext, TItemId> DefineWall<TGameContext, TItemId>(this ItemDeclarationBuilderWithReferenceContext<TGameContext, TItemId> b)
            where TItemId : IEntityKey
        {
            return b.Define("Wall");
        }
    }
}