using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Common
{
    public interface ISenseDataView
    {
        bool TryQuery(int x,
                      int y,
                      out float intensity,
                      out SenseDirectionStore directionality);
    }
    
    
    [SuppressMessage("ReSharper", "PossibleInterfaceMemberAmbiguity")]
    public interface IDynamicSenseDataView2D: IReadOnlyDynamicDataView2D<float>, IReadOnlyDynamicDataView2D<SenseDirectionStore>, ISenseDataView
    {
        new int OffsetX { get; }
        new int OffsetY { get; }
        new int TileSizeX { get; }
        new int TileSizeY { get; }

        new List<Rectangle> GetActiveTiles(List<Rectangle> data = null);
        new Rectangle GetActiveBounds();
    }
}