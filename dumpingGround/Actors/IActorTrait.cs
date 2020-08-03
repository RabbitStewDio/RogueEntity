using EnttSharp.Entities;

namespace ValionRL.Core.Infrastructure.Meta.Actors
{
    public interface IActorTrait<TContext, TActorId>: ITrait where TActorId : IEntityKey
    {
        /// <summary>
        ///   This method is called right after an character has been spawned. Use this for
        ///   your first time set up.
        /// </summary>
        void Initialize(IEntityViewControl<TActorId> v, TContext context, TActorId k);

        /// <summary>
        ///   This method is called after an character's composition has changed. This is called after an
        ///   item has been added or removed from the character and is used to recompute the current
        ///   character stats.
        /// </summary>
        void Apply(IEntityViewControl<TActorId> v, TContext context, TActorId k);
    }
}