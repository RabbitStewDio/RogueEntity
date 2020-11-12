using System;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public readonly struct ModuleId : IEquatable<ModuleId>
    {
        public readonly string Id;

        public ModuleId(string id)
        {
            Id = id;
        }

        public bool Equals(ModuleId other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is ModuleId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }

        public static bool operator ==(ModuleId left, ModuleId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ModuleId left, ModuleId right)
        {
            return !left.Equals(right);
        }
        
        public static implicit operator ModuleId(string s)
        {
            return new ModuleId(s);
        }

        public override string ToString()
        {
            return Id;
        }
    }
}