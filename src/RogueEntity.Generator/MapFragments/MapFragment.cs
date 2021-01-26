using RogueEntity.Api.Utils;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;

namespace RogueEntity.Generator.MapFragments
{
    public readonly struct MapFragment : IEquatable<MapFragment>
    {
        public IReadOnlyView2D<MapFragmentTagDeclaration> MapData { get; }
        public MapFragmentInfo Info { get; }
        public TypedRuleProperties Properties { get; }
        public Guid Id { get; }
        public Dimension Size { get; }

        public MapFragment(Guid id, IReadOnlyView2D<MapFragmentTagDeclaration> mapData, MapFragmentInfo info, Dimension size, TypedRuleProperties properties)
        {
            Id = id;
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

        public MapFragment WithMapData(IReadOnlyView2D<MapFragmentTagDeclaration> mapData, Dimension size, Optional<Guid> id = default)
        {
            if (id.TryGetValue(out var newId))
            {
                return new MapFragment(newId, mapData, Info, size, Properties);
            }
            
            return new MapFragment(Id, mapData, Info, size, Properties);
        }
        
        public MapFragment WithProperties(TypedRuleProperties properties, Optional<Guid> id = default)
        {
            if (id.TryGetValue(out var newId))
            {
                return new MapFragment(newId, MapData, Info, Size, properties);
            }
            
            return new MapFragment(Id, MapData, Info, Size, properties);
        }
        
        public MapFragment WithName(string name, Optional<Guid> id = default)
        {
            if (id.TryGetValue(out var newId))
            {
                return new MapFragment(newId, MapData, Info.WithName(name), Size, Properties);
            }
            
            var newIdFromName = GuidUtility.Create(GuidUtility.UrlNamespace, name);
            return new MapFragment(newIdFromName, MapData, Info.WithName(name), Size, Properties);
        }
    }
}
