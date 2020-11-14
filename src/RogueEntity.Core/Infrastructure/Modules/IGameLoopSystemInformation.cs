using System.Collections.Generic;
using RogueEntity.Core.Infrastructure.GameLoops;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public interface IGameLoopSystemInformation<TGameContext>
    {
        public IEnumerable<ActionSystemEntry<TGameContext>> InitializationEntries { get; }
        public IEnumerable<ActionSystemEntry<TGameContext>> PreFixedStepEntries { get; }
        public IEnumerable<ActionSystemEntry<TGameContext>> FixedStepEntries { get; }
        public IEnumerable<ActionSystemEntry<TGameContext>> LateFixedStepEntries { get; }

        public IEnumerable<ActionSystemEntry<TGameContext>> VariableStepEntries { get; }

        public IEnumerable<ActionSystemEntry<TGameContext>> LateVariableStepEntries { get; }

        public IEnumerable<ActionSystemEntry<TGameContext>> DisposeEntries { get; }
    }
}