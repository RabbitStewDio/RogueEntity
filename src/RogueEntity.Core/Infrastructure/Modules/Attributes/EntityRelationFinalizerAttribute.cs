using System;
using JetBrains.Annotations;

namespace RogueEntity.Core.Infrastructure.Modules.Attributes
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
    public class EntityRelationFinalizerAttribute: Attribute
    {
        public string RelationName { get; }
        public string[] ConditionalSubjectRoles { get; set; }
        public string[] ConditionalObjectRoles { get; set; }
        
        public EntityRelationFinalizerAttribute(string roleName)
        {
            RelationName = roleName;
            ConditionalObjectRoles = new string[0];
            ConditionalSubjectRoles = new string[0];
        }
    }
}