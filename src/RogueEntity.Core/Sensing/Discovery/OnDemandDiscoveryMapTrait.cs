using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Sensing.Discovery
{
    public class OnDemandDiscoveryMapTrait<TGameContext, TActorId> : DiscoveryMapTrait<TGameContext, TActorId, OnDemandDiscoveryMap>
        where TActorId : IEntityKey
    {
        readonly int width;
        readonly int height;

        public OnDemandDiscoveryMapTrait(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
        
        protected override OnDemandDiscoveryMap CreateValue(TGameContext context, TActorId k, IItemDeclaration item)
        {
            return new OnDemandDiscoveryMap(width, height);
        }
    }
}