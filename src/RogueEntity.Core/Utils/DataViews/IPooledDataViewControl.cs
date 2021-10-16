using RogueEntity.Api.Utils;

namespace RogueEntity.Core.Utils.DataViews
{
    public interface IPooledDataViewControl
    {
        void PrepareFrame(long time);
        void ExpireFrames(long age);
        void ExpireAll();
    }

    public interface IPooledDataViewControl2D: IPooledDataViewControl
    {
        BufferList<Rectangle> GetDirtyTiles(BufferList<Rectangle> data = null);
    }

    public interface IPooledDataViewControl3D : IPooledDataViewControl
    {
        bool RemoveView(int z);
    }
}
