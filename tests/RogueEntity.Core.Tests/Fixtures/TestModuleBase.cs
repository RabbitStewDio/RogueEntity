using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Tests.Fixtures
{
    public class TestModuleBase : ModuleBase
    {
        readonly List<Action<ModuleInitializationParameter, IModuleInitializer>> contentInitializers;
        readonly List<Action<ModuleInitializationParameter,IModuleInitializer>> moduleInitializers;
        readonly List<Action<ModuleInitializationParameter,IModuleInitializer>> lateModuleInitializers;

        public TestModuleBase(ModuleId moduleId)
        {
            Id = moduleId;
            
            contentInitializers = new List<Action<ModuleInitializationParameter, IModuleInitializer>>();
            moduleInitializers = new List<Action<ModuleInitializationParameter, IModuleInitializer>>();
            lateModuleInitializers = new List<Action<ModuleInitializationParameter, IModuleInitializer>>();
        }

        public TestModuleBase AddContentInitializer(Action<ModuleInitializationParameter, IModuleInitializer> ci)
        {
            contentInitializers.Add(ci);
            return this;
        }

        public TestModuleBase AddModuleInitializer(Action<ModuleInitializationParameter, IModuleInitializer> ci)
        {
            moduleInitializers.Add(ci);
            return this;
        }

        public TestModuleBase AddLateModuleInitializer(Action<ModuleInitializationParameter, IModuleInitializer> ci)
        {
            lateModuleInitializers.Add(ci);
            return this;
        }

        [ModuleInitializer]
        void InitializeModule(in ModuleInitializationParameter mip, IModuleInitializer initializer)
        {
            foreach (var m in moduleInitializers)
            {
                m(mip, initializer);
            }
        }
        
        [LateModuleInitializer]
        void InitializeLateModule(in ModuleInitializationParameter mip, IModuleInitializer initializer)
        {
            foreach (var m in lateModuleInitializers)
            {
                m(mip, initializer);
            }
        }
        
        [ContentInitializer]
        void InitializeContent(in ModuleInitializationParameter mip, IModuleInitializer initializer)
        {
            var mipCopy = mip;
            foreach (var c in contentInitializers)
            {
                c(mipCopy, initializer);
            }
        }
    }
}
