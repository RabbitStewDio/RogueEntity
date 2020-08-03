using System.Collections.Generic;
using EnTTSharp.Annotations;
using EnttSharp.Entities;

namespace ValionRL.Core.Infrastructure.Meta.Actors
{
    /// <summary>
    ///   Actors are all entities that can move on their own. Movement is controlled by
    ///   action points, which recover at each turn.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TActorId"></typeparam>
    [EntityComponent(EntityConstructor.NonConstructable)]
    public interface IActorDefinition<TContext, TActorId> : IWorldEntity where TActorId : IEntityKey
    {
        ActorDefinitionId Id { get; }

        bool TryQuery<TTrait>(out TTrait t) where TTrait : IActorTrait<TContext, TActorId>;
        List<TTrait> QueryAll<TTrait>(List<TTrait> cache = null) where TTrait : IActorTrait<TContext, TActorId>;

        void Initialize(IEntityViewControl<TActorId> v, TContext context, TActorId k);
        void Apply(IEntityViewControl<TActorId> v, TContext context, TActorId k);
    }
}