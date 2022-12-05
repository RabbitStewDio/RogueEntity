using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Positioning
{
    /// <summary>
    ///    A semi-hidden helper interface intended to be implemented by a GridMapContext implementation
    ///    to allow modules to trigger a map reset. This allows us to trigger a reset of *all* the map
    ///    data whilst still letting developers provide their own implementation if they choose to.  
    /// </summary>
    /// <typeparam name="TItemId">Only used as selector.</typeparam>
    [SuppressMessage("ReSharper", "UnusedTypeParameter")]
    public interface IMapContextInitializer<TItemId>
    {
        void ResetState();
    }
}
