using System;
using JetBrains.Annotations;

namespace RogueEntity.Core.Infrastructure.Modules
{
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class EntityRelationInitializerAttribute: Attribute
    {
        public string RelationName { get; }

        public EntityRelationInitializerAttribute(string roleName)
        {
            RelationName = roleName;
        }
    }
}