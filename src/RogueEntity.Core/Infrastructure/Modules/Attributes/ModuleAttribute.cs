using System;
using JetBrains.Annotations;

namespace RogueEntity.Core.Infrastructure.Modules.Attributes
{
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ModuleAttribute : Attribute
    {
    }
}