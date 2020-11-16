using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;

namespace RogueEntity.Core.Sensing.Receptors
{
    public static class SenseReceptorModules
    {
        public static EntityRole GetReceptorRole<TReceptorSense, TSourceSense>() => new EntityRole($"Role.Core.Senses.Receptor.{typeof(TReceptorSense).Name}.{typeof(TSourceSense).Name}.Sensor");

        public static EntitySystemId CreateSystemId<TReceptorSense, TSourceSense>(string job) =>
            new EntitySystemId($"Core.Systems.Senses.{typeof(TReceptorSense).Name}.Receptor.{typeof(TSourceSense).Name}.{job}");

        public static EntitySystemId CreateEntityId<TReceptorSense, TSourceSense>(string job) =>
            new EntitySystemId($"Entities.Systems.Senses.{typeof(TReceptorSense).Name}.Receptor.{typeof(TSourceSense).Name}.{job}");
    }
}