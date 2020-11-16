using System;
using JetBrains.Annotations;

namespace RogueEntity.Api.Modules.Attributes
{
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