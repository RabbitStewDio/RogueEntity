using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Sensing.Sources.Noise
{
    public static class NoisyItemExtensions
    {
        public static void PlayNoise<TItemId>(this IItemResolver<TItemId> resolver,
                                              TItemId item,
                                              in NoiseClip clip)
            where TItemId : struct, IEntityKey
        {
            resolver.TryUpdateData(item, in clip, out _);
        }
    }
}
