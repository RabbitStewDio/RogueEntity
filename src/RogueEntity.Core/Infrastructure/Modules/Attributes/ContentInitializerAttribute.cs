using System;
using JetBrains.Annotations;

namespace RogueEntity.Core.Infrastructure.Modules.Attributes
{
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class ContentInitializerAttribute: Attribute
    {
    }
}