using System.Collections.Generic;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public abstract class ModuleBase<TGameContext> : IModule<TGameContext>
    {
        readonly List<string> moduleDependencies;

        protected ModuleBase()
        {
            moduleDependencies = new List<string>();
        }

        public string Id { get; protected set; }
        public string Name { get; protected set; }
        public string Author { get; protected set; }
        public string Description { get; protected set; }

        public IEnumerable<string> ModuleDependencies
        {
            get { return moduleDependencies; }
        }

        protected void DeclareDependencies(params string[] dependencies)
        {
            moduleDependencies.AddRange(dependencies);
        }

        public virtual void Initialize(TGameContext context, IModuleInitializer<TGameContext> initializer)
        {
        }
    }
}