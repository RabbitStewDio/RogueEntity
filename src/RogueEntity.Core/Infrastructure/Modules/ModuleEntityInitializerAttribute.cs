using System;

namespace RogueEntity.Core.Infrastructure.Modules
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ModuleEntityInitializerAttribute: Attribute
    {
    }
}