using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Meta.Naming;
using RogueEntity.Core.Meta.UseEffects;
using Serilog;

namespace RogueEntity.Core.Effects.Uses
{
    public class MorphItemOnUseEffect<TGameContext, TActorId, TItemId> : IUsableItemEffect<TGameContext, TActorId, TItemId> 
        where TItemId : IEntityKey 
        where TActorId : IEntityKey
        where TGameContext: IItemContext<TGameContext, TItemId>
    {
        readonly ItemDeclarationId morphTarget;
        readonly bool preserveDurability;
        readonly bool preserveStackSize;

        public MorphItemOnUseEffect(ItemDeclarationId morphTarget, 
                                    bool preserveDurability = true, 
                                    bool preserveStackSize = true)
        {
            this.morphTarget = morphTarget;
            this.preserveDurability = preserveDurability;
            this.preserveStackSize = preserveStackSize;
        }

        public bool TryActivate(TActorId user, TGameContext context, TItemId itemToBeUsed, out TItemId usedItem)
        {
            var itemResolver = context.ItemResolver;

            if (!itemResolver.ItemRegistry.TryGetItemById(morphTarget, out var itemDeclaration))
            {
                usedItem = itemToBeUsed;
                return false;
            }

            var itemReference = itemResolver.Instantiate(context, itemDeclaration);

            if (preserveDurability && itemDeclaration.HasItemComponent<TGameContext, TItemId, Durability>())
            {
                if (!TryPreserveDurability(itemToBeUsed, context, itemReference, out var itemWithDurability))
                {
                    itemResolver.DiscardUnusedItem(itemReference);
                    usedItem = itemToBeUsed;
                    return false;
                }

                itemReference = itemWithDurability;
            }

            if (preserveStackSize && itemDeclaration.HasItemComponent<TGameContext, TItemId, StackCount>())
            {
                if (!TryPreserveStackCount(itemToBeUsed, context, itemReference, out var itemWithStackCount))
                {
                    itemResolver.DiscardUnusedItem(itemReference);
                    usedItem = itemToBeUsed;
                    return false;
                }

                itemReference = itemWithStackCount;
            }

            itemResolver.Destroy(itemToBeUsed);
            Log.Debug("Replaced {SourceItem} with {ReplacementItem}",
                      itemToBeUsed.ToItemName(context),
                      itemReference.ToItemName(context));
            usedItem = itemReference;
            return true;
        }

        bool TryPreserveDurability(TItemId itemToBeUsed, TGameContext context, TItemId itemReference, out TItemId changedItem)
        {
            var itemResolver = context.ItemResolver;
            if (!itemResolver.TryQueryData(itemToBeUsed, context, out Durability durabilityData))
            {
                changedItem = itemReference;
                return true;
            }

            return itemResolver.TryUpdateData(itemReference, context, in durabilityData, out changedItem);
        }

        bool TryPreserveStackCount(TItemId itemToBeUsed, TGameContext context, TItemId itemReference, out TItemId changedItem)
        {
            var itemResolver = context.ItemResolver;
            if (!itemResolver.TryQueryData(itemToBeUsed, context, out StackCount durabilityData))
            {
                changedItem = itemReference;
                return true;
            }

            return itemResolver.TryUpdateData(itemReference, context, in durabilityData, out changedItem);
        }
    }
}