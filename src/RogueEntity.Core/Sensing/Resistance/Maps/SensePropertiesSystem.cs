using System.Diagnostics.CodeAnalysis;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    /// <summary>
    ///   A tagging interface to make the dependency injection select the right value.
    /// </summary>
    /// <typeparam name="TSense"></typeparam>
    [SuppressMessage("ReSharper", "UnusedTypeParameter", Justification = "Discriminator")]
    public interface ISensePropertiesDataView<TSense> : IAggregationLayerSystem<float>
    { }


    public class SensePropertiesSystem<TSense> : LayeredAggregationSystem<float, SensoryResistance<TSense>>,
                                                 ISensePropertiesDataView<TSense>
    {
        public SensePropertiesSystem(int tileWidth, int tileHeight) : base(SensePropertiesSystem.ProcessTile, tileWidth, tileHeight)
        { }

        public SensePropertiesSystem(int offsetX, int offsetY, int tileSizeX, int tileSizeY) : base(SensePropertiesSystem.ProcessTile, offsetX, offsetY, tileSizeX, tileSizeY)
        { }
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
                    if (!dv.TryGet(x, y, out var senseResistancePercent))
                    {
                        continue;
                    }

                    if (resistanceData.TryGet(x, y, out var resistance))
                    {
                        var sp = resistance + senseResistancePercent.ToFloat();
                        resistanceData.TrySet(x, y, Percentage.Of(sp));
                    }
                    else
                    {
                        resistanceData.TrySet(x, y, Percentage.Full);
                    }
                }
            }
        }
    }
}
