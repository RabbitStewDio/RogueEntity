using EnTTSharp;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;

namespace RogueEntity.Generator.MapFragments
{
    public readonly struct MapFragment : IEquatable<MapFragment>
    {
        public IReadOnlyView2D<MapFragmentTagDeclaration> MapData { get; }
        public MapFragmentInfo Info { get; }
        public TypedRuleProperties Properties { get; }
        public Guid Id { get; }
        public List<MapFragmentTagDeclaration> Symbols { get; }
        public Dimension Size { get; }

        public MapFragment(Guid id, 
                           List<MapFragmentTagDeclaration> symbols,
                           IReadOnlyView2D<MapFragmentTagDeclaration> mapData, 
                           MapFragmentInfo info, 
                           Dimension size, 
                           TypedRuleProperties properties)
        {
            Id = id;
            Symbols = new List<MapFragmentTagDeclaration>(symbols);
            Properties = properties;
            MapData = mapData;
            Info = info;
            Size = size;
        }

        public override string ToString()
        {
            return $"Fragment({Info.Name})";
        }

        public override bool Equals(object obj)
        {
            return obj is MapFragment other && Equals(other);
        }

        public bool Equals(MapFragment other)
        {
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(MapFragment left, MapFragment right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MapFragment left, MapFragment right)
        {
            return !left.Equals(right);
        }

        public MapFragment WithMapData(List<MapFragmentTagDeclaration> symbols, 
                                       IReadOnlyView2D<MapFragmentTagDeclaration> mapData, 
                                       Dimension size, 
                                       Optional<Guid> id = default)
        {
            if (id.TryGetValue(out var newId))
            {
                return new MapFragment(newId, symbols, mapData, Info, size, Properties);
            }
            
            return new MapFragment(Id, symbols, mapData, Info, size, Properties);
        }
        
        public MapFragment WithProperties(TypedRuleProperties properties, Optional<Guid> id = default)
        {
            if (id.TryGetValue(out var newId))
            {
                return new MapFragment(newId, Symbols, MapData, Info, Size, properties);
            }
            
            return new MapFragment(Id, Symbols, MapData, Info, Size, properties);
        }
        
        public MapFragment WithName(string name, Optional<Guid> id = default)
        {
            if (id.TryGetValue(out var newId))
            {
                return new MapFragment(newId, Symbols, MapData, Info.WithName(name), Size, Properties);
            }
            
            var newIdFromName = GuidUtility.Create(GuidUtility.UrlNamespace, name);
            return new MapFragment(newIdFromName, Symbols, MapData, Info.WithName(name), Size, Properties);
        }
    }
}
