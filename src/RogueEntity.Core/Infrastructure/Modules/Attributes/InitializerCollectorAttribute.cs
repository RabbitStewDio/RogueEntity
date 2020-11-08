using System;
using JetBrains.Annotations;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public enum InitializerCollectorType
    {
        Roles = 0,
        Relations = 1
    }
    
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class InitializerCollectorAttribute: Attribute
    {
        public InitializerCollectorType Type { get; set; }

        public InitializerCollectorAttribute(InitializerCollectorType type)
        {
            Type = type;
        }
    }
}