using JetBrains.Annotations;
using System;

namespace RogueEntity.Api.Modules.Attributes
{
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class LateModuleInitializerAttribute: Attribute
    {
    }
}
