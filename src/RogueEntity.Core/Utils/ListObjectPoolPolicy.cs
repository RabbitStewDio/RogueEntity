using Microsoft.Extensions.ObjectPool;
using System.Collections.Generic;

namespace RogueEntity.Core.Utils
{
    public class ListObjectPoolPolicy<T>: IPooledObjectPolicy<List<T>>
    {
        public List<T> Create()
        {
            return new List<T>();
        }

        public bool Return(List<T> obj)
        {
            obj.Clear();
            return true;
        }
    }
}