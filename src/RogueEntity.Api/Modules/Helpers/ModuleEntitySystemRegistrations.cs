using System;
using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.Utils;
using Serilog;

namespace RogueEntity.Api.Modules.Helpers
{
    public class ModuleEntitySystemRegistrations : IGameLoopSystemRegistration,
                                                   IGameLoopSystemInformation
    {
        static readonly ILogger Logger = SLog.ForContext<ModuleEntitySystemRegistrations>();
        ISystemDeclaration CurrentSystem { get; set; }
        string contextPattern;

        readonly List<ActionSystemEntry> initializationEntries;
        readonly List<ActionSystemEntry> preFixedStepEntries;
        readonly List<ActionSystemEntry> fixedStepEntries;
        readonly List<ActionSystemEntry> lateFixedStepEntries;
        readonly List<ActionSystemEntry> variableStepEntries;
        readonly List<ActionSystemEntry> lateVariableStepEntries;
        readonly List<ActionSystemEntry> disposeEntries;

        public ModuleEntitySystemRegistrations()
        {
            initializationEntries = new List<ActionSystemEntry>();
            preFixedStepEntries = new List<ActionSystemEntry>();
            fixedStepEntries = new List<ActionSystemEntry>();
            lateFixedStepEntries = new List<ActionSystemEntry>();
            variableStepEntries = new List<ActionSystemEntry>();
            lateVariableStepEntries = new List<ActionSystemEntry>();
            disposeEntries = new List<ActionSystemEntry>();
        }

        public void EnterContext(IGlobalSystemDeclaration d)
        {
            Logger.Debug("Processing global system {SystemId} in module {ModuleId}", d.Id, d.DeclaringModule);
            CurrentSystem = d;
            contextPattern = "#";
        }

        public void EnterContext<TEntity>(IEntitySystemDeclaration<TEntity> d)
            where TEntity : IEntityKey
        {
            Logger.Debug("Processing entity system {SystemId} in module {ModuleId} for {EntityId}", d.Id, d.DeclaringModule, typeof(TEntity));
            CurrentSystem = d;
            contextPattern = typeof(TEntity).Name;
        }

        public void LeaveContext()
        {
            CurrentSystem = null;
            contextPattern = null;
        }
        
        ActionSystemEntry Wrap(Action c, string description, int order)
        {
            var d = string.IsNullOrEmpty(description) ? EntitySystemReference.CreateSystemDescription(c) : description;
            return new ActionSystemEntry(c, CurrentSystem, order, $"{contextPattern}|{d}");
        }

        void AddEntry(List<ActionSystemEntry> entries,
                      Action c,
                      string description)
        {
            var entry = Wrap(c, description, entries.Count);
            entries.Add(entry);
        }

        public void AddInitializationStepHandler(Action c, string description = null)
        {
            AddEntry(initializationEntries, c, description);
        }

        public void AddPreFixedStepHandlers(Action c, string description = null)
        {
            AddEntry(preFixedStepEntries, c, description);
        }

        public void AddFixedStepHandlers(Action c, string description = null)
        {
            AddEntry(fixedStepEntries, c, description);
        }

        public void AddLateFixedStepHandlers(Action c, string description = null)
        {
            AddEntry(lateFixedStepEntries, c, description);
        }

        public void AddVariableStepHandlers(Action c, string description = null)
        {
            AddEntry(variableStepEntries, c, description);
        }

        public void AddLateVariableStepHandlers(Action c, string description = null)
        {
            AddEntry(lateVariableStepEntries, c, description);
        }

        public void AddDisposeStepHandler(Action c, string description = null)
        {
            AddEntry(disposeEntries, c, description);
        }

        public IEnumerable<ActionSystemEntry> InitializationEntries
        {
            get { return initializationEntries.OrderBy(e => (e.Priority, e.DeclarationOrder)); }
        }

        public IEnumerable<ActionSystemEntry> PreFixedStepEntries
        {
            get { return preFixedStepEntries.OrderBy(e => (e.Priority, e.DeclarationOrder)); }
        }

        public IEnumerable<ActionSystemEntry> FixedStepEntries
        {
            get { return fixedStepEntries.OrderBy(e => (e.Priority, e.DeclarationOrder)); }
        }

        public IEnumerable<ActionSystemEntry> LateFixedStepEntries
        {
            get { return lateFixedStepEntries.OrderBy(e => (e.Priority, e.DeclarationOrder)); }
        }

        public IEnumerable<ActionSystemEntry> VariableStepEntries
        {
            get { return variableStepEntries.OrderBy(e => (e.Priority, e.DeclarationOrder)); }
        }

        public IEnumerable<ActionSystemEntry> LateVariableStepEntries
        {
            get { return lateVariableStepEntries.OrderBy(e => (e.Priority, e.DeclarationOrder)); }
        }

        public IEnumerable<ActionSystemEntry> DisposeEntries
        {
            get { return disposeEntries.OrderBy(e => (e.Priority, e.DeclarationOrder)); }
        }
    }
}
