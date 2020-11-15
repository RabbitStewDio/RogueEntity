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
}