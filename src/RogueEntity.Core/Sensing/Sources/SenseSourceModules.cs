using EnTTSharp.Entities;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Helpers;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Sources
{
    public static class SenseSourceModules
    {
        public static EntityRole GetSourceRole<TSense>() => new EntityRole($"Role.Core.Senses.Source.{typeof(TSense).Name}.SenseSource");
        public static EntityRole GetResistanceRole<TSense>() => new EntityRole($"Role.Core.Senses.{typeof(TSense).Name}.ResistanceProvider");
        public static EntityRelation GetResistanceRelation<TSense>() => new EntityRelation($"Relation.Core.Senses.Resistance.{typeof(TSense).Name}.ProvidesResistanceData", GetResistanceRole<TSense>(), GetSourceRole<TSense>());
        public static EntitySystemId CreateSystemId<TSense>(string job) => new EntitySystemId($"Core.Systems.Senses.Source.{typeof(TSense).Name}.{job}");
        public static EntitySystemId CreateEntityId<TSense>(string job) => new EntitySystemId($"Entities.Systems.Senses.Source.{typeof(TSense).Name}.{job}");
        
        public static EntitySystemId CreateResistanceSourceSystemId<TSense>() => new EntitySystemId($"Core.Systems.Senses.Resistance.{typeof(TSense).Name}.ConfigureResistanceDataProvider");

        /// <summary>
        ///    Performs the necessary setup to feed sensory resistance data from items stored on a grid map into the sensory resistance aggregator. 
        /// </summary>
        /// <param name="layers"></param>
        /// <typeparam name="TGameContext"></typeparam>
        /// <typeparam name="TItemId"></typeparam>
        /// <typeparam name="TSense"></typeparam>
        /// <returns></returns>
        public static EntitySystemRegistrationDelegate<TGameContext, TItemId> RegisterSenseResistanceSourceLayer<TGameContext, TItemId, TSense>(ReadOnlyListWrapper<MapLayer> layers)
            where TItemId : IEntityKey
        {
            void RegisterItemResistanceSystemConfiguration(in ModuleInitializationParameter initParameter,
                                                           IGameLoopSystemRegistration<TGameContext> context,
                                                           EntityRegistry<TItemId> registry)
            {
                var serviceResolver = initParameter.ServiceResolver;
                var itemContext = serviceResolver.Resolve<IItemResolver<TGameContext, TItemId>>();
                var mapContext = serviceResolver.Resolve<IGridMapContext<TItemId>>();
                
                var factory = serviceResolver.Resolve<IAggregationLayerSystem<TGameContext, SensoryResistance<TSense>>>();
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