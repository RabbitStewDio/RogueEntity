using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing.Sources
{
    public interface ISenseDefinition
    {
        bool Enabled { get; }
        SenseSourceDefinition SenseDefinition { get; }
    }
}