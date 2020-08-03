using System.Collections.Generic;
using EnttSharp.Entities;
using Serilog;

namespace ValionRL.Core.Infrastructure.Meta.Actors
{
    /// <summary>
    ///   An resolver interface to allow efficitent lookups and trait operations.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TActorId"></typeparam>
    public class ActorResolver<TContext, TActorId> : IActorResolver<TContext, TActorId>
        where TActorId : IEntityKey
    {
        static readonly ILogger Logger = SLog.ForContext<ActorResolver<TContext, TActorId>>();

        readonly EntityRegistry<TActorId> entityRegistry;

        public ActorResolver(EntityRegistry<TActorId> entityRegistry)
        {
            this.entityRegistry = entityRegistry;
        }

        public TActorId Instantiate(TContext context,
                                    IActorDefinition<TContext, TActorId> actorDeclaration)
        {
            var itemReference = entityRegistry.Create();
            entityRegistry.AssignComponent(itemReference, actorDeclaration);
            actorDeclaration.Initialize(entityRegistry, context, itemReference);
            return itemReference;
        }

        public bool TryResolve(in TActorId actorRef, out IActorDefinition<TContext, TActorId> item)
        {
            if (entityRegistry.IsValid(actorRef) &&
                entityRegistry.GetComponent(actorRef, out IActorDefinition<TContext, TActorId> ri))
            {
                item = ri;
                return true;
            }

            item = default;
            return false;
        }

        bool TryQueryTrait<TActorData>(in TActorId actorRef, out TActorData data)
            where TActorData : IActorTrait<TContext, TActorId>
        {
            if (TryResolve(actorRef, out var itemDeclaration))
            {
                return itemDeclaration.TryQuery(out data);
            }

            data = default;
            return false;
        }

        public bool TryQueryData<TData>(in TActorId actorRef, TContext context, out TData data)
        {
            if (TryQueryTrait<IActorComponentTrait<TContext, TActorId, TData>>(actorRef, out var trait))
            {
                return trait.TryQuery(entityRegistry, context, actorRef, out data);
            }

            data = default;
            return false;
        }

        public bool TryUpdateData<TData>(in TActorId actorRef,
                                         TContext context,
                                         in TData data)
        {
            if (TryQueryTrait<IActorComponentTrait<TContext, TActorId, TData>>(actorRef, out var trait))
            {
                return trait.TryUpdate(entityRegistry, context, actorRef, in data);
            }

            Logger.Warning("Attempt to update data {Data} on {Actor} failed due to missing component trait", data, actorRef);
            return false;
        }

        public void DiscardUnusedActor(in TActorId actor)
        {
            if (entityRegistry.IsValid(actor))
            {
                entityRegistry.Destroy(actor);
            }
        }

        public void Apply(in TActorId reference, TContext context)
        {
            if (entityRegistry.IsValid(reference) &&
                entityRegistry.GetComponent(reference, out IActorDefinition<TContext, TActorId> ri))
            {
                ri.Apply(this.entityRegistry, context, reference);
            }
        }

        public void Destroy(in TActorId actor)
        {
            if (entityRegistry.IsValid(actor))
            {
                entityRegistry.AssignOrReplace<DestroyedMarker>(actor);
            }
        }

        public IEnumerable<TActorId> FindAllInstancesWithTrait<TTrait>() where TTrait : IActorTrait<TContext, TActorId>
        {
            foreach (var ek in entityRegistry.View<IActorDefinition<TContext, TActorId>>())
            {
                if (entityRegistry.GetComponent(ek, out IActorDefinition<TContext, TActorId> i) &&
                    i.TryQuery(out TTrait _))
                {
                    yield return ek;
                }
            }
        }
    }
}