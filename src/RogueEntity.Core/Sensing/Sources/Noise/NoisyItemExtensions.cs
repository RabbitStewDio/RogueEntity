using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Sensing.Sources.Noise
{
    public static class NoisyItemExtensions
    {
        public static void PlayNoise<TGameContext, TItemId>(this ItemResolver<TGameContext, TItemId> resolver,
                                                            TItemId item,
                                                            TGameContext context,
                                                            in NoiseClip clip)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            resolver.TryUpdateData(item, context, in clip, out _);
        }
    }
}