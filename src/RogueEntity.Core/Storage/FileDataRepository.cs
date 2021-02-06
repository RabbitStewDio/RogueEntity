using MessagePack;
using RogueEntity.Api.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;

namespace RogueEntity.Core.Storage
{
    public class FileDataRepository<TKey, TData> : IDataRepository<TKey, TData>
    {
        static readonly ILogger Logger = SLog.ForContext<FileDataRepository<TKey, TData>>();

        readonly IFileKeyConverter<TKey> fileKeyConverter;
        readonly MessagePackSerializerOptions messagePackSerializerOptions;
        readonly string baseDirectory;
        readonly DirectoryInfo dirInfo;

        public FileDataRepository(IFileKeyConverter<TKey> fileKeyConverter,
                                  MessagePackSerializerOptions messagePackSerializerOptions,
                                  string baseDirectory)
        {
            this.fileKeyConverter = fileKeyConverter;
            this.messagePackSerializerOptions = messagePackSerializerOptions;
            this.baseDirectory = baseDirectory;
            this.dirInfo = Directory.CreateDirectory(baseDirectory);
        }

        public IEnumerable<TKey> QueryEntries()
        {
            foreach (var f in dirInfo.GetFiles())
            {
                if (fileKeyConverter.TryParseFromFileName(f.Name, out var key))
                {
                    yield return key;
                }
            }
        }

        public bool TryRead(in TKey k, out TData value)
        {
            if (!fileKeyConverter.TryConvertToFileName(k, out var filename))
            {
                value = default;
                return false;
            }

            try
            {
                using var fs = File.OpenRead(Path.Combine(baseDirectory, filename));
                value = MessagePackSerializer.Deserialize<TData>(fs, messagePackSerializerOptions);
                return true;
            }
            catch (Exception e)
            {
                Logger.Information(e, "Unable to deserialize stored profile data for key {Key}", k);
                value = default;
                return false;
            }
        }

        public bool TryStore(in TKey k, in TData value)
        {
            if (!fileKeyConverter.TryConvertToFileName(k, out var filename))
            {
                return false;
            }

            var fullPath = Path.Combine(baseDirectory, filename);
            var tmp = Path.GetTempFileName();
            try
            {
                using var fs = File.Create(tmp);
                
                MessagePackSerializer.Serialize(fs, value, messagePackSerializerOptions);
                fs.Close();
                File.Delete(fullPath);
                File.Move(tmp, fullPath);
                return true;
            }
            catch (Exception e)
            {
                try
                {
                    File.Delete(tmp);
                }
                catch
                {
                    // Ignored completely.
                }

                Logger.Information(e, "Unable to serialize stored profile data for key {Key}", k);
                return false;
            }
        }

        public bool TryDelete(in TKey k)
        {
            if (!fileKeyConverter.TryConvertToFileName(k, out var filename))
            {
                return false;
            }

            try
            {
                var fullPath = Path.Combine(baseDirectory, filename);
                File.Delete(fullPath);
                return true;
            }
            catch(Exception e)
            {
                Logger.Information(e, "Unable to serialize stored profile data for key {Key}", k);
                return false;
            }
        }
    }
}
