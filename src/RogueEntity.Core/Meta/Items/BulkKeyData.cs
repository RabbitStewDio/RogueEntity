using System.Runtime.Serialization;

namespace RogueEntity.Core.Meta.Items
{
    [DataContract]
    public readonly struct BulkKeyData
    {
        [DataMember(Name = "IsReference")]
        public readonly bool IsReference;
        [DataMember(Name = "ItemId")]
        public readonly int ItemId;
        [DataMember(Name = "Age")]
        public readonly byte Age;
        [DataMember(Name = "Data")]
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