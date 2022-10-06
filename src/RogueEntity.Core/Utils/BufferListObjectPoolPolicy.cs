using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.Utils;

namespace RogueEntity.Core.Utils;

public class BufferListObjectPoolPolicy<T>: IPooledObjectPolicy<BufferList<T>>
{
    public BufferList<T> Create()
    {
        return new BufferList<T>();
    }

    public bool Return(BufferList<T> obj)
    {
        obj.Clear();
        return true;
    }
}