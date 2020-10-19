using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public interface IModuleInitializer<TGameContext>
    {
        IModuleEntityContext<TGameContext, TEntityId> DeclareEntityContext<TEntityId>() where TEntityId : IEntityKey;
        
        void Register(EntitySystemId id,
                      int priority,
                      ModuleEntityContext.GlobalSystemRegistrationDelegate<TGameContext> entityRegistration);
    }

    public interface IModuleEntityContext<TGameContext, TEntityId> where TEntityId : IEntityKey
    {
        IEnumerable<IBulkItemDeclaration<TGameContext, TEntityId>> DeclaredBulkItems { get; }
        IEnumerable<IReferenceItemDeclaration<TGameContext, TEntityId>> DeclaredReferenceItems { get; }
        IEnumerable<IEntitySystemFactory<TGameContext, TEntityId>> EntitySystems { get; }

        ItemDeclarationId Declare(IBulkItemDeclaration<TGameContext, TEntityId> item);
        ItemDeclarationId Declare(IReferenceItemDeclaration<TGameContext, TEntityId> item);

        void Register(EntitySystemId id,
                      int priority,
                      ModuleEntityContext.EntityRegistrationDelegate<TEntityId> entityRegistration);

        void Register(EntitySystemId id,
                      int priority,
                      ModuleEntityContext.EntitySystemRegistrationDelegate<TGameContext, TEntityId> entitySystemRegistration = null);
    }
    
    public static class ModuleEntityContextExtensions 
    {
        public static ModuleEntityContext.EntitySystemRegistrationDelegate<TGameContext, TEntityId> Empty<TGameContext, TEntityId>(this IModuleEntityContext<TGameContext, TEntityId> ctx)
            where TEntityId : IEntityKey
        {
            return (a, b, c, d) =>
            {
            };
        }
    }
}