using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Meta.ItemBuilder
{
    public readonly struct ItemDeclarationInfo
    {
        public readonly string Tag;
        public readonly ItemDeclarationId Id;

        public ItemDeclarationInfo(ItemDeclarationId id, string tag)
        {
            Id = id;
            Tag = tag ?? Id.Id;
        }

        public static ItemDeclarationInfo Of(ItemDeclarationId id, string tag = null)
        {
            return new ItemDeclarationInfo(id, tag);
        }
    }
}
