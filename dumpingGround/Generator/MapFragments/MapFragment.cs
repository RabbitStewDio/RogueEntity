﻿using RogueEntity.Core.Utils.Maps;
using ValionRL.Core.MapFragments;

namespace RogueEntity.Generator.MapFragments
{
    public readonly struct MapFragment
    {
        public IReadOnlyMapData<MapFragmentTagDeclaration> MapData { get; }
        public MapFragmentInfo Info { get; }

        public MapFragment(IReadOnlyMapData<MapFragmentTagDeclaration> mapData, MapFragmentInfo info)
        {
            MapData = mapData;
            Info = info;
        }

        public override string ToString()
        {
            return $"Fragment({Info.Name})";
        }
    }
}