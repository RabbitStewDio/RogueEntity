using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Storage
{
    public interface IFileKeyConverter<TValue>
    {
        public bool TryParseFromFileName(string path, [MaybeNullWhen(false)] out TValue value);
        public bool TryConvertToFileName(TValue key, [MaybeNullWhen(false)] out string path);
    }
}
