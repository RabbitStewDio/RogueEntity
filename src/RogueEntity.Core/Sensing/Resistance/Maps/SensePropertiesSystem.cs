using System.Diagnostics.CodeAnalysis;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    /// <summary>
    ///   A tagging interface to make the dependency injection select the right value.
    /// </summary>
    /// <typeparam name="TSense"></typeparam>
    /// <typeparam name="TGameContext"></typeparam>
    [SuppressMessage("ReSharper", "UnusedTypeParameter", Justification = "Discriminator")]
    public interface ISensePropertiesDataView<TSense> : IAggregationLayerSystem<float>
    {
        
    }
    
    
    public class SensePropertiesSystem<TGameContext, TSense> : LayeredAggregationSystem<TGameContext, float, SensoryResistance<TSense>>, 
                                                               ISensePropertiesDataView<TSense>
    {
        public SensePropertiesSystem(int tileWidth, int tileHeight) : base(SensePropertiesSystem.ProcessTile, tileWidth, tileHeight)
        {
        }

        public SensePropertiesSystem(int offsetX, int offsetY, int tileSizeX, int tileSizeY) : base(SensePropertiesSystem.ProcessTile, offsetX, offsetY, tileSizeX, tileSizeY)
        {
        }
    }

    public static class SensePropertiesSystem
    {
        public static void ProcessTile<TSense>(AggregationProcessingParameter<float, SensoryResistance<TSense>> p)
        {
            var bounds = p.Bounds;
            var resistanceData = p.WritableTile;
            resistanceData.Clear();

            foreach (var view in p.DataViews)
            {
                if (!view.TryGetData(bounds.X, bounds.Y, out var dv))
                {
                    continue;
                }

                foreach (var (x, y) in bounds.Contents)
                {
                    if (!dv.TryGet(x, y, out var d))
                    {
                        continue;
                    }

                    var sp = resistanceData[x, y] + d.BlocksSense.RawData;
                    resistanceData.TrySet(x, y, Percentage.Of(sp));
                }
            }
        }
    }
}