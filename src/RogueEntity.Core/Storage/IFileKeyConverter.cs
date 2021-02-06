namespace RogueEntity.Core.Storage
{
    public interface IFileKeyConverter<TValue>
    {
        public bool TryParseFromFileName(string path, out TValue value);
        public bool TryConvertToFileName(TValue key, out string path);
    }
}
