using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Common
{
    public interface ISenseDataView: IReadOnlyDynamicDataView2D<float>, IReadOnlyDynamicDataView2D<SenseDirectionStore>
    {
        new int OffsetX { get; }
        new int OffsetY { get; }
        new int TileSizeX { get; }
        new int TileSizeY { get; }

        new List<Rectangle> GetActiveTiles(List<Rectangle> data = null);
        new Rectangle GetActiveBounds();
        
        float QueryBrightness(int x, int y);
        SenseDirectionStore QueryDirection(int x, int y);
        
        bool TryQuery(int x,
                      int y,
                      out float intensity,
                      out SenseDirectionStore directionality);
    }
}