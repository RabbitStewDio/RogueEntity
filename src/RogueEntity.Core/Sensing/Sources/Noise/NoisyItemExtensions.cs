using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Sensing.Sources.Noise
{
    public static class NoisyItemExtensions
    {
        public static void PlayNoise<TGameContext, TItemId>(this IItemResolver<TGameContext, TItemId> resolver,
                                                            TItemId item,
                                                            TGameContext context,
                                                            in NoiseClip clip)
            where TItemId : IEntityKey
        {
            resolver.TryUpdateData(item, context, in clip, out _);
        }
    }
}