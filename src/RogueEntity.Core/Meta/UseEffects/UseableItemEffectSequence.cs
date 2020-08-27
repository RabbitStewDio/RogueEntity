using System.Collections.Generic;
using EnTTSharp.Entities;

namespace RogueEntity.Core.Meta.UseEffects
{
    public class UseableItemEffectSequence<TGameContext, TActorId, TItemId> : IUsableItemEffect<TGameContext, TActorId, TItemId>
        where TItemId : IEntityKey
        where TActorId : IEntityKey
    {
        readonly List<IUsableItemEffect<TGameContext, TActorId, TItemId>> effects;

        public UseableItemEffectSequence(params IUsableItemEffect<TGameContext, TActorId, TItemId>[] effects)
        {
            this.effects = new List<IUsableItemEffect<TGameContext, TActorId, TItemId>>(effects);
        }

        public UseableItemEffectSequence(IEnumerable<IUsableItemEffect<TGameContext, TActorId, TItemId>> effects)
        {
            this.effects = new List<IUsableItemEffect<TGameContext, TActorId, TItemId>>(effects);
        }

        public bool TryActivate(TActorId user, TGameContext context, TItemId itemToBeUsed, out TItemId usedItem)
        {
            usedItem = itemToBeUsed;
            bool result = false;
            foreach (var e in effects)
            {
                if (e.TryActivate(user, context, usedItem, out var item))
                {
                    usedItem = item;
                    result = true;
                }
            }

            return result;
        }
    }
}