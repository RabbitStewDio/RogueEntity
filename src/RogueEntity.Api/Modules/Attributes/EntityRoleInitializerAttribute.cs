using System;
using JetBrains.Annotations;

namespace RogueEntity.Api.Modules.Attributes
{
    /// <summary>
    ///   Used to set up aspects of an entity role. Entity roles are bound to an entity registry.
    ///
    ///   <![CDATA[
    ///   void InitializeItemRole<TItemId>(IServiceResolver serviceResolver, 
    ///                                                  IModuleInitializer initializer,
    ///                                                  EntityRole r)
    ///    ]]>
    /// </summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class EntityRoleInitializerAttribute : Attribute
    {
        public string RoleName { get; }
        public string[] ConditionalRoles { get; set; }
        public string[] ConditionalRelations { get; set; }
        public string[] WithAnyRoles { get; set; }

        public EntityRoleInitializerAttribute(string roleName)
        {
            RoleName = roleName;
            ConditionalRoles = Array.Empty<string>();
            ConditionalRelations = Array.Empty<string>();
            WithAnyRoles = Array.Empty<string>();
        }
    }
}
