using System.Runtime.Serialization;
using EnTTSharp.Entities;
using EnTTSharp.Entities.Attributes;
using EnTTSharp.Serialization.Binary.AutoRegistration;
using RogueEntity.Api.ItemTraits;

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
        where TItemId : IEntityKey
    {
        public readonly IReferenceItemDeclaration<TItemId> ItemDeclaration;

        public ItemDeclarationHolder(IReferenceItemDeclaration<TItemId> itemDeclaration)
        {
            this.ItemDeclaration = itemDeclaration;
        }

        public string Id => ItemDeclaration.Id.Id;
    }
}