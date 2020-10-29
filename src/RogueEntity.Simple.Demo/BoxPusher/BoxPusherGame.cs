using RogueEntity.Core.Infrastructure.Modules;
using Serilog;

namespace RogueEntity.Simple.Demo.BoxPusher
{
    public class BoxPusherGame
    {
        public static void BoxMain()
        {
            Log.Debug("Starting");
            
            var ms = new ModuleSystem<BoxPusherContext>();
            ms.ScanForModules();

            var context = new BoxPusherContext(128, 128);
            ms.Initialize(context);
        }    
    }
}