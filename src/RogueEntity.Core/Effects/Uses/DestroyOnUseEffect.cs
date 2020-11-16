using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.UseEffects;
using Serilog;

namespace RogueEntity.Core.Effects.Uses
{
    public class DestroyOnUseEffect<TGameContext, TActorId, TItemId> : IUsableItemEffect<TGameContext, TActorId, TItemId> 
        where TItemId : IEntityKey 
        where TActorId : IEntityKey
    {
        static readonly ILogger Logger = SLog.ForContext<DestroyOnUseEffect<TGameContext, TActorId, TItemId>>();
        readonly IItemResolver<TGameContext, TItemId> itemResolver;

        public bool TryActivate(TActorId user, TGameContext context, TItemId itemToBeUsed, out TItemId usedItem)
        {
            Logger.Debug($"Destroyed item: {itemToBeUsed}");

            itemResolver.Destroy(itemToBeUsed);
            usedItem = default;
            return true;
        }
    }
}