using ValionRL.Core.Infrastructure.Actions2;

namespace RogueEntity.Core.Modules
{
    public static class DumpExtensions
    {
        public static ActionPointRecoveryTime Build(this ActionPointRecoveryDefinition def)
        {
            return new ActionPointRecoveryTime(def.Magnitude, def.Frequency, 0);
        }
    }
}