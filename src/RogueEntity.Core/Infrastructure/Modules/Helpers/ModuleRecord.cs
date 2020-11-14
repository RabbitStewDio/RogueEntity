using System.Collections.Generic;
using System.Linq;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Infrastructure.Modules.Helpers
{
    public class ModuleRecord<TGameContext>
    {
        public static readonly ILogger Logger = SLog.ForContext<ModuleSystem<TGameContext>>();
        public readonly ModuleId ModuleId;
        public readonly ModuleBase Module;
        public readonly List<ModuleRecord<TGameContext>> Dependencies;
        public bool IsUsedAsDependency { get; set; }
        public bool ResolvedRoles { get; set; }
        public bool ResolvedEquivalence { get; set; }
        public bool ResolvedOrder { get; set; }
        public bool InitializedModule { get; set; }
        public bool InitializedContent { get; set; }
        public bool InitializedRoles { get; set; }
        public bool InitializedRelations { get; set; }
        public bool FinalizedRoles { get; set; }
        public bool FinalizedRelations { get; set; }

        public int DependencyDepth
        {
            get
            {
                if (Dependencies.Count == 0)
                {
                    return 0;
                }

                return Dependencies.Max(d => d.DependencyDepth) + 1;
            }
        }

        public ModuleRecord(ModuleBase module)
        {
            Module = module;
            ModuleId = module.Id;
            Dependencies = new List<ModuleRecord<TGameContext>>();
        }

        public override string ToString()
        {
            return $"{nameof(ModuleId)}: {ModuleId}";
        }

        public void AddDependency(ModuleRecord<TGameContext> value, string dependencyType)
        {
            foreach (var d in Dependencies)
            {
                if (d.ModuleId == value.ModuleId)
                {
                    return;
                }
            }

            Dependencies.Add(value);
            Logger.Debug("Added Module dependency from {SourceModuleId} to {DependencyModuleId} as {DependencyType}", ModuleId, value.ModuleId, dependencyType);
        }
    }
}