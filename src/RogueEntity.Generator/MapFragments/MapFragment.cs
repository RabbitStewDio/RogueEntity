using RogueEntity.Api.Utils;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;
using System;

namespace RogueEntity.Generator.MapFragments
{
    public readonly struct MapFragment : IEquatable<MapFragment>
    {
        public IReadOnlyMapData<MapFragmentTagDeclaration> MapData { get; }
        public MapFragmentInfo Info { get; }
        public TypedRuleProperties Properties { get; }
        public Guid Id { get; }

        public MapFragment(Guid id, IReadOnlyMapData<MapFragmentTagDeclaration> mapData, MapFragmentInfo info, TypedRuleProperties properties)
        {
            Id = id;
            Properties = properties;
            MapData = mapData;
            Info = info;
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

        public MapFragment WithMapData(IReadOnlyMapData<MapFragmentTagDeclaration> mapData, Optional<Guid> id = default)
        {
            if (id.TryGetValue(out var newId))
            {
                return new MapFragment(newId, mapData, Info, Properties);
            }
            
            return new MapFragment(Id, mapData, Info, Properties);
        }
        
        public MapFragment WithProperties(TypedRuleProperties properties, Optional<Guid> id = default)
        {
            if (id.TryGetValue(out var newId))
            {
                return new MapFragment(newId, MapData, Info, properties);
            }
            
            return new MapFragment(Id, MapData, Info, properties);
        }
        
        public MapFragment WithName(string name, Optional<Guid> id = default)
        {
            if (id.TryGetValue(out var newId))
            {
                return new MapFragment(newId, MapData, Info.WithName(name), Properties);
            }
            
            var newIdFromName = GuidUtility.Create(GuidUtility.UrlNamespace, name);
            return new MapFragment(newIdFromName, MapData, Info.WithName(name), Properties);
        }
    }
}
