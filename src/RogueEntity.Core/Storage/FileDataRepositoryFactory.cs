using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;

namespace RogueEntity.Core.Storage
{
    public class FileDataRepositoryFactory : IDataRepositoryFactory
    {
        readonly string profileDirectory;
        readonly MessagePackSerializerOptions options;
        readonly Dictionary<Type, object> converters;

        public FileDataRepositoryFactory(string profileDirectory, 
                                         MessagePackSerializerOptions options)
        {
            Directory.CreateDirectory(profileDirectory);

            this.converters = new Dictionary<Type, object>();
            this.profileDirectory = profileDirectory;
            this.options = options;
        }

        public FileDataRepositoryFactory WithKey<TKey>(IFileKeyConverter<TKey> valueConverter)
        {
            this.converters[typeof(TKey)] = valueConverter ?? throw new ArgumentNullException(nameof(valueConverter));
            return this;
        }

        public IDataRepository<TKey, TData> Create<TKey, TData>(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Id must not be an empty string");
            }

            if (id.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                throw new ArgumentException("Id contains invalid characters");
            }

            if (!converters.TryGetValue(typeof(TKey), out var convert) ||
                !(convert is IFileKeyConverter<TKey> fileConverter))
            {
                throw new ArgumentException("Unable to locate key converter for type " + typeof(TKey));
            }

            var path = Path.Combine(profileDirectory, id);
            if (!path.StartsWith(profileDirectory))
            {
                throw new ArgumentException($"Repository-id '{id}' is not valid.");
            }

            return new FileDataRepository<TKey, TData>(fileConverter, Path.Combine(profileDirectory, id), options);
        }
    }
}
