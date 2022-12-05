using EnTTSharp.Entities;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Helpers;
using RogueEntity.Api.Utils;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;

namespace RogueEntity.Core.Sensing.Sources
{
    public static class SenseSourceModules
    {
        public static EntityRole GetSourceRole<TSense>() => new EntityRole($"Role.Core.Senses.Source.{typeof(TSense).Name}.SenseSource");
        public static EntityRole GetResistanceRole<TSense>() => new EntityRole($"Role.Core.Senses.{typeof(TSense).Name}.ResistanceProvider");
        
        public static EntityRelation GetResistanceRelation<TSense>() => new EntityRelation($"Relation.Core.Senses.Resistance.{typeof(TSense).Name}.ProvidesResistanceData", 
                                                                                           GetResistanceRole<TSense>(), GetSourceRole<TSense>());
        
        public static EntitySystemId CreateSystemId<TSense>(string job) => new EntitySystemId($"Core.Systems.Senses.Source.{typeof(TSense).Name}.{job}");
        public static EntitySystemId CreateEntityId<TSense>(string job) => new EntitySystemId($"Entities.Systems.Senses.Source.{typeof(TSense).Name}.{job}");
        
        public static EntitySystemId CreateResistanceSourceSystemId<TSense>() => new EntitySystemId($"Core.Systems.Senses.Resistance.{typeof(TSense).Name}.ConfigureResistanceDataProvider");

        /// <summary>
        ///    Performs the necessary setup to feed sensory resistance data from items stored on a grid map into the sensory resistance aggregator. 
        /// </summary>
        /// <param name="layers"></param>
        /// <typeparam name="TItemId"></typeparam>
        /// <typeparam name="TSense"></typeparam>
        /// <returns></returns>
        public static EntitySystemRegistrationDelegate< TItemId> RegisterSenseResistanceSourceLayer< TItemId, TSense>(ReadOnlyListWrapper<MapLayer> layers)
            where TItemId : struct, IEntityKey
        {
            void RegisterItemResistanceSystemConfiguration(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                           IGameLoopSystemRegistration context,
                                                           EntityRegistry<TItemId> registry)
            {
                var serviceResolver = initParameter.ServiceResolver;
                var itemContext = serviceResolver.Resolve<IItemResolver< TItemId>>();
                var mapContext = serviceResolver.Resolve<IMapContext<TItemId>>();
                
                var factory = serviceResolver.Resolve<IAggregationLayerSystemBackend< SensoryResistance<TSense>>>();
                var cache = serviceResolver.Resolve<ISenseCacheSetupSystem>();
                foreach (var layer in layers)
                {
                    factory.AddLayer(mapContext, itemContext, layer);
                    cache.RegisterCacheLayer(layer);
                }
            }

            return RegisterItemResistanceSystemConfiguration;
        }

    }
}