using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Meta.ItemBuilder
{
    public readonly struct ItemDeclarationInfo
    {
        public readonly WorldEntityTag Tag;
        public readonly ItemDeclarationId Id;

        public ItemDeclarationInfo(ItemDeclarationId id, WorldEntityTag tag)
        {
            Id = id;
            Tag = tag.Tag == null ? new WorldEntityTag(id.Id) : tag;
        }

        public static ItemDeclarationInfo Of(ItemDeclarationId id, string? tag = default)
        {
            return new ItemDeclarationInfo(id, new WorldEntityTag(tag ?? id.Id));
        }
        
        public static ItemDeclarationInfo Of(ItemDeclarationId id, WorldEntityTag tag = default)
        {
            return new ItemDeclarationInfo(id, tag);
        }
    }
}
