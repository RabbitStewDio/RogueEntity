using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Sensing.Cache
{
    public interface ISenseCacheSetupSystem
    {
        void RegisterCacheLayer(MapLayer layer);
    }
    
    public class SenseCacheSetUpSystem: ISenseCacheSetupSystem
    {
        readonly Lazy<SenseStateCache> cacheProvider;
        readonly HashSet<MapLayer> layers;
        readonly HashSet<Type> senses;

        public SenseCacheSetUpSystem([NotNull] Lazy<SenseStateCache> cacheProvider)
        {
            this.cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
            this.layers = new HashSet<MapLayer>();
            this.senses = new HashSet<Type>();
        }

        public void RegisterCacheLayer(MapLayer layer)
        {
            layers.Add(layer);
        }

        public void RegisterSense<TSense>() where TSense: ISense
        {
            senses.Add(typeof(TSense));
        }

        public void Start()
        {
            foreach (var layer in layers)
            {
                cacheProvider.Value.ActivateGlobalCacheLayer(layer);
            }
            
            foreach (var sense in senses)
            {
                cacheProvider.Value.ActivateTrackedSenseSource(sense);
            }
        }
        
        public void Stop()
        {
        }
    }
}