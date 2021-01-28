using RogueEntity.Api.Utils;

namespace RogueEntity.Core.Utils.DataViews
{
    public interface IPooledDataViewControl
    {
        void PrepareFrame(long time);
        void ExpireFrames(long age);
        BufferList<Rectangle> GetDirtyTiles(BufferList<Rectangle> data = null);
    }
}
