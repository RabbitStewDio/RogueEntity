using System;
using JetBrains.Annotations;

namespace RogueEntity.Core.Infrastructure.Modules
{
    /// <summary>
    ///   Used to set up aspects of an entity role. Entity roles are bound to an entity registry.
    ///
    ///   <![CDATA[
    ///   void InitializeItemRole<TGameContext, TItemId>(IServiceResolver serviceResolver, 
    ///                                                  IModuleInitializer<TGameContext> initializer,
    ///                                                  EntityRole r)
    ///    ]]>
    /// </summary>
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
            ConditionalRoles = new string[0];
            ConditionalRelations = new string[0];
        }
    }
}