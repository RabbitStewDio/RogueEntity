namespace RogueEntity.Core.Effects.Uses
{
    /*
    public class ApplyActorStatusOnUseEffect<TGameContext> : IUsableItemEffect<TGameContext, TActorId, TItemId> 
        where TGameContext : IGameContext<TGameContext>, IGameContext
    {
        readonly IActorStatusEffectHandler<TGameContext> statusEffect;

        public ApplyActorStatusOnUseEffect(IActorStatusEffectHandler<TGameContext> statusEffect)
        {
            this.statusEffect = statusEffect;
        }

        protected override TItemId Activate(TActorId actor, TGameContext context, TItemId item)
        {
            if (context.ActorResolver.TryQueryData(actor, context, out ActiveActorStatusEffects<TGameContext> statusEffects))
            {
                statusEffects.Apply(context, statusEffect, item);
            }

            return item;
        }
    }
    */
}