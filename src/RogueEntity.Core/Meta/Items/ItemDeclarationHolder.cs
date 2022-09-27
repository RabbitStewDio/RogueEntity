using System.Runtime.Serialization;
using EnTTSharp.Entities;
using EnTTSharp.Entities.Attributes;
using EnTTSharp.Serialization.Binary.AutoRegistration;
using RogueEntity.Api.ItemTraits;
using System.Collections.Generic;

namespace RogueEntity.Core.Meta.Items
{
    /// <summary>
    ///   A simple wrapper/holder for reference item declarations stored in the EntityRegistry.
    ///   This wrapper simplifies the serialization and deserialization.
    /// </summary>
    /// <typeparam name="TItemId"></typeparam>
    [EntityComponent(EntityConstructor.NonConstructable)]
    [EntityBinarySerialization]
    [DataContract]
    public readonly struct ItemDeclarationHolder<TItemId>
        where TItemId : struct, IEntityKey
    {
        public readonly IReferenceItemDeclaration<TItemId> ItemDeclaration;

        public ItemDeclarationHolder(IReferenceItemDeclaration<TItemId> itemDeclaration)
        {
            this.ItemDeclaration = itemDeclaration;
        }

        public string ItemId => ItemDeclaration.Id.Id;
    }

    public class DefaultEntityTagTrait<TItemId>: StatelessItemComponentTraitBase<TItemId, WorldEntityTag>,
                                                 IItemComponentDesignTimeInformationTrait<WorldEntityTag>
        where TItemId : struct, IEntityKey
    {
        readonly WorldEntityTag tag;

        public DefaultEntityTagTrait(WorldEntityTag tag): base("ItemTrait.Generic.ItemDeclaration", 9990)
        {
            this.tag = tag;
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return CoreModule.ItemRole.Instantiate<TItemId>();
        }

        public bool TryQuery(out WorldEntityTag t)
        {
            t = tag;
            return true;
        }

        protected override WorldEntityTag GetData(TItemId k)
        {
            return tag;
        }
    }
}