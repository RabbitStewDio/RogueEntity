namespace RogueEntity.Core.Meta.ItemTraits
{
    public readonly struct WeightView
    {
        public readonly Weight BaseWeight;
        public readonly Weight InventoryWeight;
        public readonly Weight StackWeight;
        public Weight TotalWeight => StackWeight + InventoryWeight;

        public WeightView(Weight baseWeight, Weight inventoryWeight, Weight totalCarryWeight)
        {
            BaseWeight = baseWeight;
            InventoryWeight = inventoryWeight;
            StackWeight = totalCarryWeight;
        }
    }
}