using RogueEntity.Api.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueEntity.Generator.MapFragments
{
    public readonly struct MapFragmentTagDeclaration : IEquatable<MapFragmentTagDeclaration>
    {
        public static readonly MapFragmentTagDeclaration Empty = default;

        public readonly ReadOnlyListWrapper<string> Tags;

        public MapFragmentTagDeclaration(params string[] tags)
        {
            Tags = new ReadOnlyListWrapper<string>(tags.Select(Normalize).ToList());
        }
        
        public MapFragmentTagDeclaration(List<string> tags)
        {
            Tags = new ReadOnlyListWrapper<string>(tags.Select(Normalize).ToList());
        }

        static string Normalize(string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                return null;
            }

            return arg;
        }

        public bool TryGetTag(int index, out string tag)
        {
            if (index < 0 || index >= Tags.Count)
            {
                tag = default;
                return false;
            }

            tag = Tags[index];
            return true;
        }
        
        public MapFragmentTagDeclaration CombineWith(MapFragmentTagDeclaration other)
        {
            var tagsCombined = new List<string>();
            var sharedCount = Math.Min(Tags.Count, other.Tags.Count);
            for (var i = 0; i < sharedCount; i += 1)
            {
                tagsCombined.Add(Tags[i] ?? other.Tags[i]);
            }

            for (var i = sharedCount; i < Tags.Count; i += 1)
            {
                tagsCombined.Add(Tags[i]);
            }
            
            for (var i = sharedCount; i < other.Tags.Count; i += 1)
            {
                tagsCombined.Add(other.Tags[i]);
            }
            
            
            return new MapFragmentTagDeclaration(tagsCombined);
        }

        public bool Equals(MapFragmentTagDeclaration other)
        {
            return Tags.SequenceEqual(other.Tags);
        }

        public override bool Equals(object obj)
        {
            return obj is MapFragmentTagDeclaration other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hc = 0;
                for (var i = 0; i < Tags.Count; i++)
                {
                    hc = (hc * 397) ^ Tags[i]?.GetHashCode() ?? 0;
                }
                return hc;
            }
        }

        public static bool operator ==(MapFragmentTagDeclaration left, MapFragmentTagDeclaration right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MapFragmentTagDeclaration left, MapFragmentTagDeclaration right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"{Tags}";
        }
    }
}
