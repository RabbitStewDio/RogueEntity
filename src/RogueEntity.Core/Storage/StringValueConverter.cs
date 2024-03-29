﻿using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace RogueEntity.Core.Storage
{
    public class StringValueConverter : IFileKeyConverter<string>
    {
        public bool TryParseFromFileName(string path, [MaybeNullWhen(false)] out string value)
        {
            value = path;
            return true;
        }

        public bool TryConvertToFileName(string key, [MaybeNullWhen(false)] out string path)
        {
            if (key.Length > 64 || key.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                path = default;
                return false;
            }

            path = key;
            return true;
        }
    }
}
