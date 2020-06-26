using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Infrastructure.Meta
{
    public readonly struct SensoryResistance
    {
        public readonly Percentage BlocksLight;
        public readonly Percentage BlocksSound;
        public readonly Percentage BlocksHeat;
        public readonly Percentage BlocksSmell;

        public SensoryResistance(Percentage blocksLight, 
                                 Percentage blocksSound, 
                                 Percentage blocksHeat, 
                                 Percentage blocksSmell)
        {
            BlocksLight = blocksLight;
            BlocksSound = blocksSound;
            BlocksHeat = blocksHeat;
            BlocksSmell = blocksSmell;
        }

        public override string ToString()
        {
            return $"{nameof(BlocksLight)}: {BlocksLight}, {nameof(BlocksSound)}: {BlocksSound}, {nameof(BlocksHeat)}: {BlocksHeat}, {nameof(BlocksSmell)}: {BlocksSmell}";
        }
    }
}