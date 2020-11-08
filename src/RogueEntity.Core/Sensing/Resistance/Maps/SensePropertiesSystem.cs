using RogueEntity.Core.GridProcessing.LayerAggregation;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public class SensePropertiesSystem<TGameContext, TSense> : LayeredAggregationSystem<TGameContext, SensoryResistance<TSense>>
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
        public static void ProcessTile<TSense>(AggregationProcessingParameter<SensoryResistance<TSense>> p)
        {
            var bounds = p.Bounds;
            var resistanceData = p.WritableTile;
            foreach (var (x, y) in bounds.Contents)
            {
                var sp = new SensoryResistance<TSense>();
                foreach (var dv in p.DataViews)
                {
                    if (dv.TryGet(x, y, out var d))
                    {
                        sp += d;
                    }
                }

                resistanceData.TrySet(x, y, sp);
            }
        }
    }
}