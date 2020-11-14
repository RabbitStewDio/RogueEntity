using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Infrastructure.Modules.Helpers;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public interface IModuleEntityContext<TGameContext, TEntityId>
        where TEntityId : IEntityKey
    {
        void Register(EntitySystemId id,
                      int priority,
                      EntityRegistrationDelegate<TEntityId> entityRegistration);

        void Register(EntitySystemId id,
                      int priority,
                      EntitySystemRegistrationDelegate<TGameContext, TEntityId> entitySystemRegistration = null);
    }

    public interface IModuleContentContext<TGameContext, TEntityId>
        where TEntityId : IEntityKey
    {
        bool TryGetDefinedBulkItem(ItemDeclarationId id, out IBulkItemDeclaration<TGameContext, TEntityId> item);
        bool TryGetDefinedReferenceItem(ItemDeclarationId id, out IReferenceItemDeclaration<TGameContext, TEntityId> item);

        void DefineBulkItemTemplate(IBulkItemDeclaration<TGameContext, TEntityId> item);
        void DefineReferenceItemTemplate(IReferenceItemDeclaration<TGameContext, TEntityId> item);

        ItemDeclarationId Activate(IBulkItemDeclaration<TGameContext, TEntityId> item);
        ItemDeclarationId Activate(IReferenceItemDeclaration<TGameContext, TEntityId> item);

        void DeclareTraitRoles<TItemTrait>(params EntityRole[] roles);
        void DeclareTraitRelations<TItemTrait>(params EntityRelation[] relations);
    }
}