using System.Collections.Generic;
using System.Threading.Tasks;
using GoRogue;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.MapChunks;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public class SensePropertiesMap<TGameContext> : IReadOnlyView2D<SensoryResistance>
    {
        readonly int z;
        readonly Dictionary<MapLayer, ISensePropertiesDataProcessor<TGameContext>> dependencies;
        readonly DynamicDataView<SensoryResistance> data;
        readonly DynamicBoolDataView dataDirty;
        readonly List<ISensePropertiesDataProcessor<TGameContext>> dependenciesAsList;
        bool combinerDirty;

        public SensePropertiesMap(IAddByteBlitter blitter, int z, int tileWidth, int tileHeight)
        {
            this.z = z;
            this.dependencies = new Dictionary<MapLayer, ISensePropertiesDataProcessor<TGameContext>>();
            this.data = new DynamicDataView<SensoryResistance>(tileWidth, tileHeight);
            this.dataDirty = new DynamicBoolDataView(tileWidth, tileHeight);
        }

        public bool IsDefined(MapLayer layer)
        {
            return dependencies.TryGetValue(layer, out var x);
        }

        public void AddProcess(MapLayer layer, ISensePropertiesDataProcessor<TGameContext> p)
        {
            dependencies.Add(layer, p);
            combinerDirty = true;
        }

        public void RemoveLayer(MapLayer layer)
        {
            if (dependencies.TryGetValue(layer, out var processorByMapLayer))
            {
                dependencies.Remove(layer);
                combinerDirty = true;
            }
        }

        public SensoryResistance this[int x, int y]
        {
            get
            {
                return data[x, y];
            }
        }

        public void MarkDirty(MapLayer mapLayer, EntityGridPosition pos)
        {
            if (pos.IsInvalid || pos.GridZ != z)
            {
                return;
            }

            var x = pos.GridX;
            var y = pos.GridY;
            if (mapLayer == MapLayer.Indeterminate)
            {
                dataDirty.TrySet(x, y, true);
                foreach (var p in dependencies.Values)
                {
                    p.MarkDirty(x, y);
                }
            }
            else if (dependencies.TryGetValue(mapLayer, out var processorsByLayer))
            {
                dataDirty.TrySet(x, y, true);
                processorsByLayer.MarkDirty(x, y);
            }
        }

        public void ResetDirtyFlags()
        {
            foreach (var p in dependencies.Values)
            {
                p.ResetDirtyFlags();
            }
        }

        public void Process(TGameContext c)
        {
            if (combinerDirty)
            {
                dependenciesAsList.Clear();
                foreach (var d in dependencies.Values)
                {
                    dependenciesAsList.Add(d);
                }
            }
            
            var hs = new HashSet<Rectangle>();
            foreach (var d in dependenciesAsList)
            {
                d.Process(c);
                foreach (var t in d.ProcessedTiles)
                {
                    hs.Add(t);
                }
            }


            Parallel.ForEach(hs, ProcessTile);
        }

        void ProcessTile(Rectangle bounds)
        {
            foreach (var (x, y) in bounds)
            {
                var sp = new SensoryResistance();
                foreach (var p in dependenciesAsList)
                {
                    if (p.Data.TryGet(x, y, out var d))
                    {
                        sp += d;
                    }
                }

                data.TrySet(x, y, sp);
            }
        }
    }
}