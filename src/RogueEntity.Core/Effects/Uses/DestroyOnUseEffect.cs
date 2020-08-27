using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.UseEffects;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Effects.Uses
{
    public class DestroyOnUseEffect<TGameContext, TActorId, TItemId> : IUsableItemEffect<TGameContext, TActorId, TItemId> 
        where TItemId : IEntityKey 
        where TActorId : IEntityKey
        where TGameContext: IItemContext<TGameContext, TItemId>
    {
        static readonly ILogger Logger = SLog.ForContext<DestroyOnUseEffect<TGameContext, TActorId, TItemId>>();

        public bool TryActivate(TActorId user, TGameContext context, TItemId itemToBeUsed, out TItemId usedItem)
        {
            Logger.Debug($"Destroyed item: {itemToBeUsed}");

            context.ItemResolver.Destroy(itemToBeUsed);
            usedItem = default;
            return true;
        }
    }
}