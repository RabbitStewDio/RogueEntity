using System;
using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Infrastructure.Modules.Helpers
{
    public class ModuleEntitySystemRegistrations<TGameContext> : IGameLoopSystemRegistration<TGameContext>, 
                                                                 IGameLoopSystemInformation<TGameContext>
    {
        static readonly ILogger Logger = SLog.ForContext<ModuleEntitySystemRegistrations<TGameContext>>();
        ISystemDeclaration CurrentSystem { get; set; }
        string contextPattern;

        readonly List<ActionSystemEntry<TGameContext>> initializationEntries;
        readonly List<ActionSystemEntry<TGameContext>> preFixedStepEntries;
        readonly List<ActionSystemEntry<TGameContext>> fixedStepEntries;
        readonly List<ActionSystemEntry<TGameContext>> lateFixedStepEntries;
        readonly List<ActionSystemEntry<TGameContext>> variableStepEntries;
        readonly List<ActionSystemEntry<TGameContext>> lateVariableStepEntries;
        readonly List<ActionSystemEntry<TGameContext>> disposeEntries;

        public ModuleEntitySystemRegistrations()
        {
            initializationEntries = new List<ActionSystemEntry<TGameContext>>();
            preFixedStepEntries = new List<ActionSystemEntry<TGameContext>>();
            fixedStepEntries = new List<ActionSystemEntry<TGameContext>>();
            lateFixedStepEntries = new List<ActionSystemEntry<TGameContext>>();
            variableStepEntries = new List<ActionSystemEntry<TGameContext>>();
            lateVariableStepEntries = new List<ActionSystemEntry<TGameContext>>();
            disposeEntries = new List<ActionSystemEntry<TGameContext>>();
        }

        public void EnterContext(IGlobalSystemDeclaration<TGameContext> d)
        {
            Logger.Debug("Processing global system {SystemId} in module {ModuleId}", d.Id, d.DeclaringModule);
            CurrentSystem = d;
            contextPattern = "#";
        }

        public void EnterContext<TEntity>(IEntitySystemDeclaration<TGameContext, TEntity> d)
            where TEntity : IEntityKey
        {
            Logger.Debug("Processing entity system {SystemId} in module {ModuleId} for {EntityId}", d.Id , d.DeclaringModule, typeof(TEntity));
            CurrentSystem = d;
            contextPattern = typeof(TEntity).Name;
        }

        public void LeaveContext()
        {
            CurrentSystem = null;
            contextPattern = null;
        }

        ActionSystemEntry<TGameContext> Wrap(Action<TGameContext> c, string description, int order)
        {
            var d = string.IsNullOrEmpty(description) ? "" : "|" + description;
            return new ActionSystemEntry<TGameContext>(c, CurrentSystem, order, $"{contextPattern}{d}");
        }

        void AddEntry(List<ActionSystemEntry<TGameContext>> entries,
                      Action<TGameContext> c,
                      string description)
        {
            var entry = Wrap(c, description, entries.Count);
            entries.Add(entry);
        }

        public void AddInitializationStepHandler(Action<TGameContext> c, string description = null)
        {
            AddEntry(initializationEntries, c, description);
        }

        public void AddPreFixedStepHandlers(Action<TGameContext> c, string description = null)
        {
            AddEntry(preFixedStepEntries, c, description);
        }

        public void AddFixedStepHandlers(Action<TGameContext> c, string description = null)
        {
            AddEntry(fixedStepEntries, c, description);
        }

        public void AddLateFixedStepHandlers(Action<TGameContext> c, string description = null)
        {
            AddEntry(lateFixedStepEntries, c, description);
        }

        public void AddVariableStepHandlers(Action<TGameContext> c, string description = null)
        {
            AddEntry(variableStepEntries, c, description);
        }

        public void AddLateVariableStepHandlers(Action<TGameContext> c, string description = null)
        {
            AddEntry(lateVariableStepEntries, c, description);
        }

        public void AddDisposeStepHandler(Action<TGameContext> c, string description = null)
        {
            AddEntry(disposeEntries, c, description);
        }

        public IEnumerable<ActionSystemEntry<TGameContext>> InitializationEntries
        {
            get { return initializationEntries.OrderBy(e => (e.Priority, e.DeclarationOrder)); }
        }

        public IEnumerable<ActionSystemEntry<TGameContext>> PreFixedStepEntries
        {
            get { return preFixedStepEntries.OrderBy(e => (e.Priority, e.DeclarationOrder)); }
        }

        public IEnumerable<ActionSystemEntry<TGameContext>> FixedStepEntries
        {
            get { return fixedStepEntries.OrderBy(e => (e.Priority, e.DeclarationOrder)); }
        }

        public IEnumerable<ActionSystemEntry<TGameContext>> LateFixedStepEntries
        {
            get { return lateFixedStepEntries.OrderBy(e => (e.Priority, e.DeclarationOrder)); }
        }

        public IEnumerable<ActionSystemEntry<TGameContext>> VariableStepEntries
        {
            get { return variableStepEntries.OrderBy(e => (e.Priority, e.DeclarationOrder)); }
        }

        public IEnumerable<ActionSystemEntry<TGameContext>> LateVariableStepEntries
        {
            get { return lateVariableStepEntries.OrderBy(e => (e.Priority, e.DeclarationOrder)); }
        }

        public IEnumerable<ActionSystemEntry<TGameContext>> DisposeEntries
        {
            get { return disposeEntries.OrderBy(e => (e.Priority, e.DeclarationOrder)); }
        }
    }
}