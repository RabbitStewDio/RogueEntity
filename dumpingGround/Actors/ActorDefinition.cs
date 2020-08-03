using System;
using System.Collections.Generic;
using EnttSharp.Entities;

namespace ValionRL.Core.Infrastructure.Meta.Actors
{
    public class ActorDefinition<TContext, TActorId>: IActorDefinition<TContext, TActorId>, 
                                                      IEquatable<ActorDefinition<TContext, TActorId>> 
        where TActorId : IEntityKey
    {
        readonly TraitRegistration<IActorTrait<TContext, TActorId>> traits;

        public ActorDefinitionId Id { get; }
        public string Tag { get; }
        
        public ActorDefinition(ActorDefinitionId id, string tag)
        {
            traits = new TraitRegistration<IActorTrait<TContext, TActorId>>(ActorTraitComparer<TContext, TActorId>.Default);

            Id = id;
            Tag = tag;
        }

        public virtual void Initialize(IEntityViewControl<TActorId> v, TContext context, TActorId k)
        {
            foreach (var actorTrait in traits)
            {
                actorTrait.Initialize(v, context, k);
            }
        }

        public virtual void Apply(IEntityViewControl<TActorId> v, TContext context, TActorId k)
        {
            foreach (var actorTrait in traits)
            {
                actorTrait.Apply(v, context, k);
            }
        }

        public ActorDefinition<TContext, TActorId> WithTrait(IActorTrait<TContext, TActorId> trait)
        {
            traits.Add(trait);
            return this;
        }

        public ActorDefinition<TContext, TActorId> WithoutTrait<TTrait>()
        {
            traits.Remove<TTrait>();
            return this;
        }

        public bool TryQuery<TTrait>(out TTrait t) where TTrait : IActorTrait<TContext, TActorId>
        {
            return traits.TryQuery(out t);
        }

        public List<TTrait> QueryAll<TTrait>(List<TTrait> cache = null) where TTrait : IActorTrait<TContext, TActorId>
        {
            return traits.QueryAll(cache);
        }

        public bool Equals(ActorDefinition<TContext, TActorId> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id == other.Id && Tag == other.Tag;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((ActorDefinition<TContext, TActorId>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Tag != null ? Tag.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(ActorDefinition<TContext, TActorId> left, ActorDefinition<TContext, TActorId> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ActorDefinition<TContext, TActorId> left, ActorDefinition<TContext, TActorId> right)
        {
            return !Equals(left, right);
        }
    }
}