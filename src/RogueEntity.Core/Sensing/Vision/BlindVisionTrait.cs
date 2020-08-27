using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Sensing.Vision
{
    public class BlindVisionTrait<TGameContext, TActorId> : SimpleItemComponentTraitBase<TGameContext, TActorId, VisibilityDetector<TGameContext, TActorId>>
        where TActorId : IBulkDataStorageKey<TActorId>
        where TGameContext : ISenseContextProvider
    {
        public BlindVisionTrait() : base("Actor.Generic.Vision", 500)
        {
        }

        protected override VisibilityDetector<TGameContext, TActorId> CreateInitialValue(TGameContext c, TActorId actor)
        {
            return new VisibilityDetector<TGameContext, TActorId>(1.5f, 1f,
                                                                  VisibilityFunctions.VisionBlock,
                                                                  VisibilityFunctions.SenseByDistance<TGameContext, TActorId>(1.5f));
        }

        public override void Apply(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            
        }
    }
}