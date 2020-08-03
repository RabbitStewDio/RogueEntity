using System.Collections.Generic;
using EnttSharp.Entities;

namespace ValionRL.Core.Infrastructure.Meta.Actors
{
    public interface IActorResolver<TContext, TActorId> where TActorId : IEntityKey
    {
        TActorId Instantiate(TContext context, IActorDefinition<TContext, TActorId> actorDeclaration);

        bool TryResolve(in TActorId actorRef, out IActorDefinition<TContext, TActorId> item);

        bool TryQueryData<TData>(in TActorId actorRef, TContext context, out TData data);
        bool TryUpdateData<TData>(in TActorId actorRef,
                                  TContext context,
                                  in TData data);

        void Apply(in TActorId reference, TContext context);

        void DiscardUnusedActor(in TActorId actor);
        void Destroy(in TActorId actor);

        IEnumerable<TActorId> FindAllInstancesWithTrait<TTrait>() where TTrait : IActorTrait<TContext, TActorId>;
    }
}