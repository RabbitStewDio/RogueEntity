using System.Runtime.Serialization;

namespace RogueEntity.Core.Meta.Items
{
    [DataContract]
    public readonly struct BulkKeyData
    {
        [DataMember(Name = "IsReference", EmitDefaultValue = false)]
        public readonly bool IsReference;
        [DataMember(Name = "ItemId")]
        public readonly int ItemId;
        [DataMember(Name = "Age", EmitDefaultValue = false)]
        public readonly byte Age;
        [DataMember(Name = "Data", EmitDefaultValue = false)]
        public readonly int Data;

        public BulkKeyData(bool isReference, int itemId, byte age, int data)
        {
            IsReference = isReference;
            ItemId = itemId;
            Age = age;
            Data = data;
        }
    }
}