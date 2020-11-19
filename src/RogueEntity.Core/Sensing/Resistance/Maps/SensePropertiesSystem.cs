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
    public interface ISensePropertiesDataView<TGameContext, TSense> : IAggregationLayerSystem<TGameContext, float>
    {
        
    }
    
    
    public class SensePropertiesSystem<TGameContext, TSense> : LayeredAggregationSystem<TGameContext, float, SensoryResistance<TSense>>, 
                                                               ISensePropertiesDataView<TGameContext, TSense>
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
            foreach (var (x, y) in bounds.Contents)
            {
                var sp = 0;
                foreach (var dv in p.DataViews)
                {
                    if (dv.TryGet(x, y, out var d))
                    {
                        sp += d.BlocksSense.RawData;
                    }
                }

                resistanceData.TrySet(x, y, Percentage.Of(sp));
            }
        }
    }
}