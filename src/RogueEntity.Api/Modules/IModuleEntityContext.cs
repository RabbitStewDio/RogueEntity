using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules.Helpers;

namespace RogueEntity.Api.Modules
{
    public interface IModuleEntityContext<TEntityId>
        where TEntityId : IEntityKey
    {
        void Register(EntitySystemId id,
                      int priority,
                      EntityRegistrationDelegate<TEntityId> entityRegistration);

        void Register(EntitySystemId id,
                      int priority,
                      EntitySystemRegistrationDelegate<TEntityId> entitySystemRegistration = null);
    }

    public interface IModuleContentContext<TEntityId>
        where TEntityId : IEntityKey
    {
        bool TryGetDefinedBulkItem(ItemDeclarationId id, out IBulkItemDeclaration<TEntityId> item);
        bool TryGetDefinedReferenceItem(ItemDeclarationId id, out IReferenceItemDeclaration<TEntityId> item);

        void DefineBulkItemTemplate(IBulkItemDeclaration<TEntityId> item);
        void DefineReferenceItemTemplate(IReferenceItemDeclaration<TEntityId> item);

        ItemDeclarationId Activate(IBulkItemDeclaration<TEntityId> item);
        ItemDeclarationId Activate(IReferenceItemDeclaration<TEntityId> item);

        void DeclareTraitRoles<TItemTrait>(params EntityRole[] roles);
        void DeclareTraitRelations<TItemTrait>(params EntityRelation[] relations);
    }
}
