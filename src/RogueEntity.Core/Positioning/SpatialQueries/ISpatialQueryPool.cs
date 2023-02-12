using RogueEntity.Core.Utils.SpatialIndex;

namespace RogueEntity.Core.Positioning.SpatialQueries;

public interface ISpatialIndex2DPool<TComponent>
{
    public ISpatialIndex2D<TComponent> Get();
    public void Return(ISpatialIndex2D<TComponent> obj);
}