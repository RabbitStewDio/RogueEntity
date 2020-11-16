using System;
using JetBrains.Annotations;

namespace RogueEntity.Api.Modules.Attributes
{
    /// <summary>
    ///    Used to mark methods that set up relations between two entity systems.
    ///
    /// <![CDATA[
    ///         protected void InitializeContainerEntities<TGameContext, TActorId, TItemId>(IServiceResolver serviceResolver, 
    ///                                                                                     IModuleInitializer<TGameContext> initializer,
    ///                                                                                     EntityRelation r)
    /// ]]>
    /// </summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class EntityRelationInitializerAttribute: Attribute
    {
        public string RelationName { get; }
        public string[] ConditionalSubjectRoles { get; set; }
        public string[] ConditionalObjectRoles { get; set; }
        
        public EntityRelationInitializerAttribute(string roleName)
        {
            RelationName = roleName;
            ConditionalObjectRoles = new string[0];
            ConditionalSubjectRoles = new string[0];
        }
    }
}