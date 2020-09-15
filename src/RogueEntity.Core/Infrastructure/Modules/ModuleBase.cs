using System;
using System.Collections.Generic;
using System.Reflection;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public abstract class ModuleBase<TGameContext> : IModule<TGameContext>
    {
        protected delegate void EntityInitializer<TEntityKey>(TGameContext context, IModuleInitializer<TGameContext> initializer);

        readonly List<ModuleDependency> moduleDependencies;
        readonly Dictionary<Type, Action<TGameContext, IModuleInitializer<TGameContext>>> typedEntityInitializers;

        protected ModuleBase()
        {
            moduleDependencies = new List<ModuleDependency>();
            typedEntityInitializers = new Dictionary<Type, Action<TGameContext, IModuleInitializer<TGameContext>>>();
        }

        public string Id { get; protected set; }
        public string Name { get; protected set; }
        public string Author { get; protected set; }
        public string Description { get; protected set; }

        public IEnumerable<ModuleDependency> ModuleDependencies
        {
            get { return moduleDependencies; }
        }

        protected void DeclareDependencies(params ModuleDependency[] dependencies)
        {
            moduleDependencies.AddRange(dependencies);
        }

        protected void RegisterTypedInitializer<TEntityKey>(EntityInitializer<TEntityKey> initializer)
        {
            typedEntityInitializers[typeof(TEntityKey)] = (c, i) => initializer(c,i);
        }

        /// <summary>
        ///   Provides a global, non-entity dependent initializer method. This is always called before
        ///   any typed initializer.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="initializer"></param>
        public virtual void InitializeContent(TGameContext context, IModuleInitializer<TGameContext> initializer)
        {
        }

        public virtual void Initialize(Type entityKey, TGameContext context, IModuleInitializer<TGameContext> initializer)
        {
            if (typedEntityInitializers.TryGetValue(entityKey, out var del))
            {
                del(context, initializer);
                return;
            }

            foreach (var m in GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                var attr = m.GetCustomAttribute<ModuleEntityInitializerAttribute>();
                if (attr == null)
                {
                    continue;
                }

                if (m.IsSameAction(entityKey, typeof(TGameContext), typeof(IModuleInitializer<TGameContext>)))
                {
                    m.Invoke(this, new object[] {entityKey, context, initializer});
                    continue;
                }
                
                if (m.IsGenericMethod && m.IsSameAction(typeof(TGameContext), typeof(IModuleInitializer<TGameContext>)))
                {
                    var genArgs = m.GetGenericArguments();
                    if (genArgs.Length == 1)
                    {
                        m.MakeGenericMethod(entityKey).Invoke(this, new object[] {context, initializer});
                    }
                }

            }

        }
    }
}