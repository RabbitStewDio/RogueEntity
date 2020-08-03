using EnttSharp.Entities;
using ValionRL.Core.CoreModule;
using ValionRL.Core.CoreModule.Traits;
using ValionRL.Core.Infrastructure.Meta.Actors;
using ValionRL.Core.Infrastructure.Traits;

namespace ValionRL.Core.Infrastructure.Meta.Naming
{
    public class DefaultActorNameTrait<TGameContext> : IActorComponentTrait<TGameContext, IDisplayName>
        where TGameContext: IGameContext<TGameContext>
    {
        public DefaultActorNameTrait(IDisplayName displayName)
        {
            this.InitialValue = displayName;
            this.Id = "Core.Actor.DisplayName";
            this.Priority = 100;
        }

        protected IDisplayName InitialValue { get; }
        
        public string Id { get; }
        public int Priority { get; }

        public bool TryQuery(IEntityViewControl v, TGameContext context, ActorReference k, out IDisplayName t)
        {
            t = InitialValue;
            return true;
        }

        void IActorTrait<TGameContext>.Initialize(IEntityViewControl v, TGameContext context, EntityKey k, in ActorReferenceInfo<TGameContext> actorInfo)
        {
        }

        void IActorTrait<TGameContext>.Apply(IEntityViewControl v, TGameContext context, EntityKey k, in ActorReferenceInfo<TGameContext> actorInfo)
        {
        }

        bool IActorComponentTrait<TGameContext, IDisplayName>.TryUpdate(IEntityViewControl v, TGameContext context, ActorReference k, in IDisplayName t)
        {
            return false;
        }
    }
}