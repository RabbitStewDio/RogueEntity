using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Discovery
{
    /// <summary>
    ///   A interface specialization to make dependency injection possible. 
    /// </summary>
    public interface IDiscoveryMap: IReadOnlyDynamicDataView3D<bool>
    {
    }
}