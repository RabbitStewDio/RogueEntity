using RogueEntity.Api.Utils;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Meta.Base
{
    /// <summary>
    ///   A general interface to query the contents of a container-like structure.
    ///   This functionality is intended to help with cleanup of nested entities
    ///   (ie to make sure that when a creature is removed, its inventory is properly
    ///   disposed of) to avoid orphaned entities in the system.
    /// </summary>
    /// <typeparam name="TItemId"></typeparam>
    public interface IContainerView<TItemId>
    {
        ReadOnlyListWrapper<TItemId> Items { get; }
    }
}
