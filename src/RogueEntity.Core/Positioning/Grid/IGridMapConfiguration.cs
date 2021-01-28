using RogueEntity.Core.Utils.DataViews;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Positioning.Grid
{
    [SuppressMessage("ReSharper", "UnusedTypeParameter", Justification = "Used as discriminator when using dependency injection.")]
    public interface IGridMapConfiguration<TItemId>
    {
        int OffsetX { get; }
        int OffsetY { get; }
        int TileSizeX { get; }
        int TileSizeY { get; }
    }

    public static class GridMapConfigurationExtensions
    {
        public static DynamicDataViewConfiguration ToConfiguration<TEntityId>(this IGridMapConfiguration<TEntityId> t)
        {
            return new DynamicDataViewConfiguration(t.OffsetX, t.OffsetY, t.TileSizeX, t.TileSizeY);
        }
    } 
    
    public class GridMapConfiguration<TItemId> : IGridMapConfiguration<TItemId>
    {
        public int OffsetX { get; }
        public int OffsetY { get; }
        public int TileSizeX { get; }
        public int TileSizeY { get; }

        public GridMapConfiguration(DynamicDataViewConfiguration config)
        {
            OffsetX = config.OffsetX;
            OffsetY = config.OffsetY;
            TileSizeX = config.TileSizeX;
            TileSizeY = config.TileSizeY;
        }

        public static implicit operator DynamicDataViewConfiguration(GridMapConfiguration<TItemId> d)
        {
            return d.ToConfiguration();
        }

        public GridMapConfiguration(int offsetX, int offsetY, int tileSizeX, int tileSizeY)
        {
            OffsetX = offsetX;
            OffsetY = offsetY;
            TileSizeX = tileSizeX;
            TileSizeY = tileSizeY;
        }
    }
}