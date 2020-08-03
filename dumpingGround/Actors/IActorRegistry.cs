using System.Collections.Generic;
using EnttSharp.Entities;

namespace ValionRL.Core.Infrastructure.Meta.Actors
{
    public interface IActorRegistry<TContext, TActorId> where TActorId : IEntityKey
    {
        bool TryGetActorDefinitionById(ActorDefinitionId id, out IActorDefinition<TContext, TActorId> item);
        IActorDefinition<TContext, TActorId> ReferenceActorDefinitionById(ActorDefinitionId id);
        IEnumerable<IActorDefinition<TContext, TActorId>> ActiveActors { get; }
    }
}