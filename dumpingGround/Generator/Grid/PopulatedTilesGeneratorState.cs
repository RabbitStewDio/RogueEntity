using System.Linq;
using RogueEntity.Core.Utils;

namespace ValionRL.Core.Generator
{
    public class PopulatedTilesGeneratorState<TGameContext> : GeneratorStateBase<TGameContext>
    {
        public PopulatedTilesGeneratorState(NodePlacementGeneratorState<TGameContext> copy) : base(copy)
        {
        }

        public float PercentFilled 
        {
            get
            {
                var values = Nodes.ToLinq().Where(e => e != null).Count(e => e.SelectedTile.HasValue);
                var allValues = Nodes.Length;
                return (float)values / allValues;
            }
        }
    }
}