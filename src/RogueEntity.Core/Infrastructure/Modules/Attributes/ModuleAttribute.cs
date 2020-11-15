using System;
using JetBrains.Annotations;

namespace RogueEntity.Core.Infrastructure.Modules.Attributes
{
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ModuleAttribute : Attribute
    {
        public string Domain { get; set; }

        public ModuleAttribute()
        {
        }

        public ModuleAttribute(string domain)
        {
            Domain = domain;
        }
    }
}