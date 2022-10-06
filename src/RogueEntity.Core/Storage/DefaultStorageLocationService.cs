using RogueEntity.Core.Utils;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace RogueEntity.Core.Storage
{
    public class DefaultStorageLocationService: IStorageLocationService
    {
        public DefaultStorageLocationService(string contentLocation, string configurationLocation)
        {
            ContentLocation = contentLocation;
            ConfigurationLocation = configurationLocation;
        }

        public string ContentLocation { get; }
        public string ConfigurationLocation { get; }

        public static DefaultStorageLocationService CreateDefault(string appId, string? contentDirectory = "Content")
        {
            Assert.NotNull(appId);
            
            var contentLocation = Path.Combine(GetApplicationLocation(), contentDirectory ?? "Content");
            var configLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appId);
            return new DefaultStorageLocationService(contentLocation, configLocation);
        }

        static string GetApplicationLocation()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Resources");
                if (Directory.Exists(location))
                {
                    return location;
                }
            }
            
            return AppDomain.CurrentDomain.BaseDirectory;
        }

    }
}
