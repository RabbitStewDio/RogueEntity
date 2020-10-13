using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Tests.Sensing
{
    public class SenseMappingTestContext : IItemContext<SenseMappingTestContext, ActorReference>
    {
        readonly ItemContextBackend<SenseMappingTestContext, ActorReference> actorBackend;

        public SenseMappingTestContext()
        {
            actorBackend = new ItemContextBackend<SenseMappingTestContext, ActorReference>(new ActorReferenceMetaData());
        }

        public ItemRegistry<SenseMappingTestContext, ActorReference> ActorRegistry
        {
            get { return actorBackend.ItemRegistry; }
        }

        public EntityRegistry<ActorReference> ActorEntityRegistry
        {
            get { return actorBackend.EntityRegistry; }
        }

        public IItemResolver<SenseMappingTestContext, ActorReference> ActorResolver
        {
            get { return actorBackend.ItemResolver; }
        }

        IItemResolver<SenseMappingTestContext, ActorReference> IItemContext<SenseMappingTestContext, ActorReference>.ItemResolver => ActorResolver;
    }
}