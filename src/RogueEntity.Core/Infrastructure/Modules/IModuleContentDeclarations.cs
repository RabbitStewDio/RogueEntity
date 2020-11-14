using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public interface IModuleContentDeclarations<TGameContext, TEntityId>
        where TEntityId : IEntityKey
    {
        IEnumerable<(ModuleId declaringModule, IBulkItemDeclaration<TGameContext, TEntityId> itemDeclaration)> DeclaredBulkItems { get; }
        IEnumerable<(ModuleId declaringModule, IReferenceItemDeclaration<TGameContext, TEntityId> itemDeclaration)> DeclaredReferenceItems { get; }
    }
}