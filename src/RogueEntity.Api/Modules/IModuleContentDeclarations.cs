using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Api.Modules
{
    public interface IModuleContentDeclarations<TEntityId>
        where TEntityId : struct, IEntityKey
    {
        IEnumerable<(ModuleId declaringModule, IBulkItemDeclaration<TEntityId> itemDeclaration)> DeclaredBulkItems { get; }
        IEnumerable<(ModuleId declaringModule, IReferenceItemDeclaration<TEntityId> itemDeclaration)> DeclaredReferenceItems { get; }
    }
}