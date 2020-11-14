using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Meta.UseEffects;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Effects.Uses
{
    public class ReduceDurabilityOnUseEffect<TGameContext, TActorId, TItemId> : IUsableItemEffect<TGameContext, TActorId, TItemId> 
        where TItemId : IEntityKey 
        where TActorId : IEntityKey
        where TGameContext : IItemContext<TGameContext, TItemId>, IRandomContext
    {

        public bool TryActivate(TActorId user, TGameContext context, TItemId itemToBeUsed, out TItemId usedItem)
        {
            if (context.ItemResolver.TryQueryData(itemToBeUsed, context, out Durability d))
            {
                var rng = context.RandomGenerator(user.ToRandomSeedSource(), itemToBeUsed.ToRandomSeedSource().AsRandomSeed());
                var roll = 1;
                var damage = d.WithAppliedDamage(roll.ClampToUnsignedShort());
                if (damage.HitPoints == 0)
                {
                    // todo: This is most probably wrong. We want items to be allowed to trigger an
                    // effect when destroyed, just as if they had been destroyed in battle.
                    usedItem = default;
                    return true;
                }

                if (context.ItemResolver.TryUpdateData(itemToBeUsed, context, damage, out var changedItem))
                {
                    usedItem = changedItem;
                    return true;
                }
            }

            usedItem = itemToBeUsed;
            return false;
        }
    }
}