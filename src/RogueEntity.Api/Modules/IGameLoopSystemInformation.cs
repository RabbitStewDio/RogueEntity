using System.Collections.Generic;
using RogueEntity.Api.GameLoops;

namespace RogueEntity.Api.Modules
{
    public interface IGameLoopSystemInformation
    {
        public IEnumerable<ActionSystemEntry> InitializationEntries { get; }
        public IEnumerable<ActionSystemEntry> PreFixedStepEntries { get; }
        public IEnumerable<ActionSystemEntry> FixedStepEntries { get; }
        public IEnumerable<ActionSystemEntry> LateFixedStepEntries { get; }

        public IEnumerable<ActionSystemEntry> VariableStepEntries { get; }

        public IEnumerable<ActionSystemEntry> LateVariableStepEntries { get; }

        public IEnumerable<ActionSystemEntry> DisposeEntries { get; }
    }
}