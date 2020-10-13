namespace RogueEntity.Core.Utils.MapChunks
{
    public interface IByteBlitterDataSource
    {
        int WordSize { get; }
        int DataLineWidth { get; }
        byte[] Data { get; }
    }
}