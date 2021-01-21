using System;

namespace RogueEntity.Api.GameLoops
{
    public interface IGameLoopSystemRegistration
    {
        void AddInitializationStepHandler(Action c, string description = null);
        void AddPreFixedStepHandlers(Action c, string description = null);
        void AddFixedStepHandlers(Action c, string description = null);
        void AddLateFixedStepHandlers(Action c, string description = null);
        void AddVariableStepHandlers(Action c, string description = null);
        void AddLateVariableStepHandlers(Action c, string description = null);
        void AddDisposeStepHandler(Action c, string description = null);
    }
}
