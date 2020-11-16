using System;
using JetBrains.Annotations;

namespace RogueEntity.Api.Modules.Attributes
{
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class ContentInitializerAttribute: Attribute
    {
    }
}