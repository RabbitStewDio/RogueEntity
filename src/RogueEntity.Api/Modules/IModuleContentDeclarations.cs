using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Api.Modules
{
    public interface IModuleContentDeclarations<TGameContext, TEntityId>
        where TEntityId : IEntityKey
    {
        IEnumerable<(ModuleId declaringModule, IBulkItemDeclaration<TGameContext, TEntityId> itemDeclaration)> DeclaredBulkItems { get; }
        IEnumerable<(ModuleId declaringModule, IReferenceItemDeclaration<TGameContext, TEntityId> itemDeclaration)> DeclaredReferenceItems { get; }
    }
}