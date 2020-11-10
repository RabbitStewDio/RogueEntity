using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using Serilog;

namespace RogueEntity.Simple.Demo.BoxPusher
{
    public class BoxPusherGame
    {
        public static void BoxMain()
        {
            Log.Debug("Starting");

            var context = new BoxPusherContext(128, 128);

            var ms = new ModuleSystem<BoxPusherContext>(new DefaultServiceResolver().WithService(context,
                                                                                                 typeof(IItemContext<BoxPusherContext, ItemReference>),
                                                                                                 typeof(IItemContext<BoxPusherContext, ActorReference>),
                                                                                                 typeof(IGridMapContext<ItemReference>),
                                                                                                 typeof(IGridMapContext<ActorReference>)
                                                        ));
            ms.ScanForModules();
            ms.Initialize(context);
        }
    }
}