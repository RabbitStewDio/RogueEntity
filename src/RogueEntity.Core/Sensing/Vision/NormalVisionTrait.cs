using RogueEntity.Core.Infrastructure.Meta.Items;

namespace RogueEntity.Core.Sensing.Vision
{
    public class NormalVisionTrait<TGameContext, TActorId> : SimpleItemComponentTraitBase<TGameContext, TActorId, VisibilityDetector<TGameContext, TActorId>>
        where TActorId : IBulkDataStorageKey<TActorId>
        where TGameContext : ISenseContextProvider
    {
        public float Radius { get; }
        public float SenseStrength { get; }

        /// <summary>
        ///   Defines how well the sense works over longer ranges. The decay
        ///   model used is exponential. Use 1 for a linear decay, less than 1
        ///   (ie 0.25) for better vision (slower decay in the near range) or
        ///   greater than 1 (ie 1.75) for a vision model that barely can see in
        ///   the far reaches. 
        /// </summary>
        public float DecayStrength { get; set; }

        public NormalVisionTrait(float radius = 5f, float senseStrength = 1f, float decayStrength = 0.25f): base("Actor.Generic.Vision", 500)
        {
            DecayStrength = decayStrength;
            Radius = radius;
            SenseStrength = senseStrength;
        }


        protected override VisibilityDetector<TGameContext, TActorId> CreateInitialValue(TGameContext c, TActorId actor)
        {
            return new VisibilityDetector<TGameContext, TActorId>(Radius, SenseStrength,
                                                                  VisibilityFunctions.VisionBlock, 
                                                                  VisibilityFunctions.SenseAll);
        }
    }
}