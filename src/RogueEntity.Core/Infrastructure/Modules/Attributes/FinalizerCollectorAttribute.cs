using System;
using JetBrains.Annotations;

namespace RogueEntity.Core.Infrastructure.Modules.Attributes
{
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class FinalizerCollectorAttribute: Attribute
    {
        public InitializerCollectorType Type { get; set; }

        public FinalizerCollectorAttribute(InitializerCollectorType type)
        {
            Type = type;
        }
    }
}