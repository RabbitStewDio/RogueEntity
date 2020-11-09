using System;

namespace RogueEntity.Core.Infrastructure.Modules.Attributes
{
    [Obsolete]
    [AttributeUsage(AttributeTargets.Method)]
    public class ModuleEntityInitializerAttribute: Attribute
    {
    }
}