using System;
using System.Collections.Generic;
using EnttSharp.Entities;
using Serilog;

namespace ValionRL.Core.Infrastructure.Meta.Actors
{
    /// <summary>
    ///   The actor registry holds all active actor definitions.
    ///
    ///   This is really nothing more than a thin, typesafe wrapper around a dictionary.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TActorId"></typeparam>
    public class ActorRegistry<TContext, TActorId> : IActorRegistry<TContext, TActorId> 
        where TActorId : IEntityKey
    {
        readonly ILogger logger = SLog.ForContext<ActorRegistry<TContext, TActorId>>();
        readonly Dictionary<ActorDefinitionId, IActorDefinition<TContext, TActorId>> actorsById;

        public ActorRegistry()
        {
            this.actorsById = new Dictionary<ActorDefinitionId, IActorDefinition<TContext, TActorId>>();
        }

        public IEnumerable<IActorDefinition<TContext, TActorId>> ActiveActors => actorsById.Values;

        public void Register(IActorDefinition<TContext, TActorId> item)
        {
            if (actorsById.ContainsKey(item.Id))
            {
                logger.Information("Redeclaration of existing actor {ActorId}", item.Id);
                actorsById[item.Id] = item;
            }
            else
            {
                actorsById.Add(item.Id, item);
            }
        }

        public bool TryGetActorDefinitionById(ActorDefinitionId id, out IActorDefinition<TContext, TActorId> item)
        {
            return actorsById.TryGetValue(id, out item);
        }

        public IActorDefinition<TContext, TActorId> ReferenceActorDefinitionById(ActorDefinitionId id)
        {
            if (TryGetActorDefinitionById(id, out var actor))
            {
                return actor;
            }

            throw new ArgumentException($"Actor type '{id}' does not exist in this actor registry.");
        }
    }
}