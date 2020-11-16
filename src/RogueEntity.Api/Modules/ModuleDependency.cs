using System;

namespace RogueEntity.Api.Modules
{
    public readonly struct ModuleDependency : IEquatable<ModuleDependency>
    {
        public readonly string ModuleId;

        ModuleDependency(string moduleId)
        {
            ModuleId = moduleId;
        }

        public static ModuleDependency Of(string moduleId)
        {
            if (moduleId == null)
            {
                throw new ArgumentNullException(nameof(moduleId));
            }

            return new ModuleDependency(moduleId);
        }

        public bool Equals(ModuleDependency other)
        {
            return string.Equals(ModuleId, other.ModuleId);
        }

        public override bool Equals(object obj)
        {
            return obj is ModuleDependency other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ModuleId?.GetHashCode() ?? 0;
        }

        public static bool operator ==(ModuleDependency left, ModuleDependency right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ModuleDependency left, ModuleDependency right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return ModuleId ?? "";
        }
    }


}