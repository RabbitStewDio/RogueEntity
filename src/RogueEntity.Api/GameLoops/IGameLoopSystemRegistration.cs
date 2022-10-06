using EnTTSharp.Entities.Systems;
using System;

namespace RogueEntity.Api.GameLoops
{
    public interface IGameLoopSystemRegistration
    {
        void AddInitializationStepHandler(Action c, string? description = null);
        void AddPreFixedStepHandlers(Action c, string? description = null);
        void AddFixedStepHandlers(Action c, string? description = null);
        void AddLateFixedStepHandlers(Action c, string? description = null);
        void AddVariableStepHandlers(Action c, string? description = null);
        void AddLateVariableStepHandlers(Action c, string? description = null);
        void AddDisposeStepHandler(Action c, string? description = null);
    }

    public static class GameLoopSystemRegistrationExtensions
    {
        public static void AddInitializationStepHandlerSystem(this IGameLoopSystemRegistration reg, EntitySystemReference system) => reg.AddInitializationStepHandler(system.System, system.SystemId);
        public static void AddPreFixedStepHandlerSystem(this IGameLoopSystemRegistration reg, EntitySystemReference system) => reg.AddPreFixedStepHandlers(system.System, system.SystemId);
        public static void AddFixedStepHandlerSystem(this IGameLoopSystemRegistration reg, EntitySystemReference system) => reg.AddFixedStepHandlers(system.System, system.SystemId);
        public static void AddLateFixedStepHandlerSystem(this IGameLoopSystemRegistration reg, EntitySystemReference system) => reg.AddLateFixedStepHandlers(system.System, system.SystemId);
        public static void AddVariableStepHandlerSystem(this IGameLoopSystemRegistration reg, EntitySystemReference system) => reg.AddVariableStepHandlers(system.System, system.SystemId);
        public static void AddLateVariableStepHandlerSystem(this IGameLoopSystemRegistration reg, EntitySystemReference system) => reg.AddLateVariableStepHandlers(system.System, system.SystemId);
        public static void AddDisposeStepHandlerSystem(this IGameLoopSystemRegistration reg, EntitySystemReference system) => reg.AddDisposeStepHandler(system.System, system.SystemId);
    }
}
