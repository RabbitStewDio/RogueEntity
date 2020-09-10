using System.Collections.Generic;
using System.Collections.ObjectModel;
using RogueEntity.Core.Utils;
using RogueEntity.Generator;
using RogueEntity.Generator.MapFragments;
using ValionRL.Core.MapFragments;

namespace ValionRL.Core.Generator
{
    public class RegularGridMapGenerator<TGameContext> : IConnectionWeightData
    {
        readonly Dictionary<MapFragmentConnectivity, int> connectionWeights;

        public RegularGridMapGenerator(TGameContext gameContext, 
                                       ReadOnlyListWrapper<MapFragment> registry)
        {
            this.Context = gameContext;
            this.Fragments = registry;
            this.connectionWeights = new Dictionary<MapFragmentConnectivity, int>();
        }

        public void UpdateConnectionWeight(MapFragmentConnectivity c, int weight)
        {
            this.connectionWeights[c] = weight;
        }

        public RegularGridMapGenerator<TGameContext> WithFilter(string filterByTag = null)
        {
            if (!string.IsNullOrEmpty(filterByTag))
            {
                Fragments = Fragments.FindAll(RegularGridMapGeneratorChain.HasTag(filterByTag));
            }

            return this;
        }

        public RegularGridMapGenerator<TGameContext> WithGridSize(int gridWidth, int gridHeight)
        {
            Fragments = Fragments.FindAll(f => IsValid(f, gridWidth, gridHeight));
            GridX = gridWidth;
            GridY = gridHeight;
            return this;
        }

        static bool IsValid(MapFragment mf, int x, int y)
        {
            return mf.MapData.Width % x == 0 && mf.MapData.Height % y == 0;
        }

        public TGameContext Context { get; }

        public int GridX { get; private set; }

        public int GridY { get; private set; }

        public int Width => Context.Map.Width;
        public int Height => Context.Map.Height;

        public ReadOnlyListWrapper<MapFragment> Fragments { get; private set; }

        public ReadOnlyDictionary<MapFragmentConnectivity, int> ConnectionWeights
        {
            get { return connectionWeights.AsReadOnly(); }
        }

        public GeneratorConfig<TGameContext> BeginGeneration()
        {
            return GeneratorConfig<TGameContext>.Create(this);
        }
    }
}