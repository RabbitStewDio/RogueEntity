using MessagePack;
using RogueEntity.Api.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;

namespace RogueEntity.Core.Storage
{
    public abstract class FileDataRepositoryBase<TKey, TData> : IDataRepository<TKey, TData>
    {
        static readonly ILogger Logger = SLog.ForContext<FileDataRepository<TKey, TData>>();
        readonly IFileKeyConverter<TKey> fileKeyConverter;
        readonly string baseDirectory;
        readonly DirectoryInfo dirInfo;

        protected FileDataRepositoryBase(IFileKeyConverter<TKey> fileKeyConverter,
                                         string baseDirectory)
        {
            this.fileKeyConverter = fileKeyConverter;
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
                ParseFromStream(out value, fs);
                return true;
            }
            catch (Exception e)
            {
                Logger.Information(e, "Unable to deserialize stored profile data for key {Key}", k);
                value = default;
                return false;
            }
        }

        protected abstract void ParseFromStream(out TData value, FileStream fs);

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
                
                WriteToStream(value, fs);
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

        protected abstract void WriteToStream(TData value, FileStream fs);

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

    public class FileDataRepository<TKey, TData> : FileDataRepositoryBase<TKey, TData>
    {
        MessagePackSerializerOptions messagePackSerializerOptions;
        
        public FileDataRepository(IFileKeyConverter<TKey> fileKeyConverter, 
                                  string baseDirectory,
                                  MessagePackSerializerOptions messagePackSerializerOptions) : base(fileKeyConverter, baseDirectory)
        {
            this.messagePackSerializerOptions = messagePackSerializerOptions;
        }
        
        protected override void WriteToStream(TData value, FileStream fs)
        {
            MessagePackSerializer.Serialize(fs, value, messagePackSerializerOptions);
        }

        protected override void ParseFromStream(out TData value, FileStream fs)
        {
            value = MessagePackSerializer.Deserialize<TData>(fs, messagePackSerializerOptions);
        }

    }
}
