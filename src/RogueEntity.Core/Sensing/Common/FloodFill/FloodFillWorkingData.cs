using JetBrains.Annotations;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Common.FloodFill
{
    public class FloodFillWorkingData
    {
        public readonly FloodFillDijkstraMap ResultMap;

        public FloodFillWorkingData(in SenseSourceDefinition sense,
                                    in Position2D origin,
                                    [NotNull] ISensePhysics sensePhysics,
                                    [NotNull] IReadOnlyView2D<float> resistanceMap)
        {
            ResultMap = FloodFillDijkstraMap.Create(in sense, in origin, sensePhysics, resistanceMap);
        }

        public void Configure(in SenseSourceDefinition sense, in Position2D origin, ISensePhysics sensePhysics, IReadOnlyView2D<float> resistanceMap)
        {
            ResultMap.Configure(in sense, in origin, sensePhysics, resistanceMap);
        }
    }
}