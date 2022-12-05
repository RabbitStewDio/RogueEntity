namespace RogueEntity.Core.Utils.DataViews;

public static class DynamicDataViewConfigurationExtensions
{
    public static DynamicDataViewConfiguration ToConfiguration<T>(this IReadOnlyDynamicDataView3D<T> view)
    {
        return new DynamicDataViewConfiguration(view.OffsetX, view.OffsetY, view.TileSizeX, view.TileSizeY);
    }
        
    public static DynamicDataViewConfiguration ToConfiguration<T>(this IReadOnlyDynamicDataView2D<T> view)
    {
        return new DynamicDataViewConfiguration(view.OffsetX, view.OffsetY, view.TileSizeX, view.TileSizeY);
    }
}