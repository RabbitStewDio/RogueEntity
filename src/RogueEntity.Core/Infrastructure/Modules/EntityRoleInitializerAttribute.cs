using System;
using JetBrains.Annotations;

namespace RogueEntity.Core.Infrastructure.Modules
{
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class EntityRoleInitializerAttribute: Attribute
    {
        public string RoleName { get; }
        public string[] ConditionalRoles { get; set; }
        public string[] ConditionalRelations { get; set; }

        public EntityRoleInitializerAttribute(string roleName)
        {
            RoleName = roleName;
        }
    }
}