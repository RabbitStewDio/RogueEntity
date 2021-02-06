using System;

namespace RogueEntity.Core.Storage
{
    public class GuidValueConverter : IFileKeyConverter<Guid>
    {
        public bool TryParseFromFileName(string path, out Guid value)
        {
            return Guid.TryParse(path, out value);
        }

        public bool TryConvertToFileName(Guid key, out string path)
        {
            path = key.ToString();
            return true;
        }
    }
}
