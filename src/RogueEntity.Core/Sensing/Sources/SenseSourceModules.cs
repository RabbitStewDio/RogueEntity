using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Infrastructure.Modules;

namespace RogueEntity.Core.Sensing.Sources
{
    public static class SenseSourceModules
    {
        public static EntityRole GetSourceRole<TSense>() => new EntityRole($"Role.Core.Senses.Source.{typeof(TSense).Name}.SenseSource");
        public static EntityRole GetResistanceRole<TSense>() => new EntityRole($"Role.Core.Senses.{typeof(TSense).Name}.ResistanceProvider");
        public static EntitySystemId CreateSystemId<TSense>(string job) => new EntitySystemId($"Core.Systems.Senses.{typeof(TSense).Name}.{job}");
        public static EntitySystemId CreateEntityId<TSense>(string job) => new EntitySystemId($"Entities.Systems.Senses.{typeof(TSense).Name}.{job}");
    }
}