using System;
using System.Collections.Generic;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public class SensePropertiesSystem<TGameContext> : ISensePropertiesSource, ISensePropertiesSystem<TGameContext>
    {
        public event EventHandler<PositionDirtyEventArgs> SenseResistancePositionDirty;

        readonly SensePropertiesLayerStore<TGameContext> propertiesMap;
        readonly List<ISenseLayerFactory<TGameContext>> layerFactories2;

        public SensePropertiesSystem(int tileWidth, int tileHeight) : this(0, 0, tileWidth, tileHeight)
        {
        }

        public SensePropertiesSystem(int offsetX, int offsetY, int tileWidth, int tileHeight)
        {
            this.OffsetX = offsetX;
            this.OffsetY = offsetY;
            this.TileWidth = tileWidth;
            this.TileHeight = tileHeight;
            this.propertiesMap = new SensePropertiesLayerStore<TGameContext>();
            this.layerFactories2 = new List<ISenseLayerFactory<TGameContext>>();
        }

        public int OffsetX { get; }
        public int OffsetY { get; }
        public int TileWidth { get; }
        public int TileHeight { get; }

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

        public ReadOnlyListWrapper<int> DefinedZLayers => propertiesMap.ZLayers;

        public void ProcessSenseProperties(TGameContext context)
        {
            foreach (var lf in layerFactories2)
            {
                lf.PrepareLayers(context, this);
            }

            propertiesMap.Process(context);
        }

        public bool TryGet(int z, out IReadOnlyView2D<SensoryResistance> data)
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

        public bool TryGetData(int z, out ISensePropertiesLayer<TGameContext> data)
        {
            if (propertiesMap.TryGetLayer(z, out var dataImpl))
            {
                data = dataImpl;
                return true;
            }

            data = default;
            return false;
        }

        public ISensePropertiesLayer<TGameContext> GetOrCreate(int z)
        {
            if (propertiesMap.TryGetLayer(z, out var dataImpl))
            {
                return dataImpl;
            }

            dataImpl = new SensePropertiesMap<TGameContext>(z, OffsetX, OffsetY, TileWidth, TileHeight);
            propertiesMap.DefineLayer(z, dataImpl);
            return dataImpl;
        }

        public void AddSenseLayerFactory(ISenseLayerFactory<TGameContext> layerHandler)
        {
            layerFactories2.Add(layerHandler);
        }
    }
}