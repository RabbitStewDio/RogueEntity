using System.Collections.Generic;
using GoRogue;

namespace RogueEntity.Core.Utils.MapChunks
{
    public interface IAddByteBlitter
    {
        int MinimumChunkSize { get; }

        void Process<TBlitter>(byte[] targetData, int lineWidth,
                               IReadOnlyList<TBlitter> sources,
                               in Rectangle area) where TBlitter: IByteBlitterDataSource;
    }
}