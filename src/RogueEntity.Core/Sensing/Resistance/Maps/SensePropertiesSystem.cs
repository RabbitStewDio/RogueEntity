using System;
using System.Collections.Generic;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public class SensePropertiesSystem<TGameContext, TSense> : IReadOnlyDynamicDataView3D<SensoryResistance<TSense>>, 
                                                               ISensePropertiesSystem<TGameContext, TSense>
    {
        public event EventHandler<PositionDirtyEventArgs> SenseResistancePositionDirty;

        readonly SensePropertiesLayerStore<TGameContext, TSense> propertiesMap;
        readonly List<ISenseLayerFactory<TGameContext, TSense>> layerFactories2;

        public SensePropertiesSystem(int tileWidth, int tileHeight) : this(0, 0, tileWidth, tileHeight)
        {
        }

        public SensePropertiesSystem(int offsetX, int offsetY, int tileSizeX, int tileSizeY)
        {
            this.OffsetX = offsetX;
            this.OffsetY = offsetY;
            this.TileSizeX = tileSizeX;
            this.TileSizeY = tileSizeY;
            this.propertiesMap = new SensePropertiesLayerStore<TGameContext, TSense>();
            this.layerFactories2 = new List<ISenseLayerFactory<TGameContext, TSense>>();
        }

        public int OffsetX { get; }
        public int OffsetY { get; }
        public int TileSizeX { get; }
        public int TileSizeY { get; }

        public void OnPositionDirty(object source, PositionDirtyEventArgs args)
        {
            if (propertiesMap.MarkDirty(EntityGridPosition.From(args.Position)))
            {
                SenseResistancePositionDirty?.Invoke(this, args);
            }
        }

        public void Start(TGameContext context)
        {
            foreach (var lf in layerFactories2)
            {
                lf.Start(context, this);
            }
        }

        public void Stop(TGameContext context)
        {
            foreach (var lf in layerFactories2)
            {
                lf.Stop(context, this);
            }

            foreach (var l in propertiesMap.ZLayers)
            {
                propertiesMap.RemoveLayer(l);
            }
        }

        public List<int> GetActiveLayers(List<int> buffer = null)
        {
            if (buffer == null)
            {
                buffer = new List<int>();
            }
            else
            {
                buffer.Clear();
            }

            foreach (var z in propertiesMap.ZLayers)
            {
                buffer.Add(z);
            }

            return buffer;
        }

        public ReadOnlyListWrapper<int> DefinedZLayers => propertiesMap.ZLayers;

        public void ProcessSenseProperties(TGameContext context)
        {
            foreach (var lf in layerFactories2)
            {
                lf.PrepareLayers(context, this);
            }

            propertiesMap.Process(context);
        }

        public bool TryGetView(int z, out IReadOnlyDynamicDataView2D<SensoryResistance<TSense>> data)
        {
            if (propertiesMap.TryGetLayer(z, out var layerData))
            {
                data = layerData;
                return true;
            }

            data = default;
            return false;
        }

        public void Remove(int z)
        {
            propertiesMap.RemoveLayer(z);
        }

        public bool TryGetData(int z, out ISensePropertiesLayer<TGameContext, TSense> data)
        {
            if (propertiesMap.TryGetLayer(z, out var dataImpl))
            {
                data = dataImpl;
                return true;
            }

            data = default;
            return false;
        }

        public ISensePropertiesLayer<TGameContext, TSense> GetOrCreate(int z)
        {
            if (propertiesMap.TryGetLayer(z, out var dataImpl))
            {
                return dataImpl;
            }

            dataImpl = new SensePropertiesMap<TGameContext, TSense>(z, OffsetX, OffsetY, TileSizeX, TileSizeY);
            propertiesMap.DefineLayer(z, dataImpl);
            return dataImpl;
        }

        public void AddSenseLayerFactory(ISenseLayerFactory<TGameContext, TSense> layerHandler)
        {
            layerFactories2.Add(layerHandler);
        }
    }
}