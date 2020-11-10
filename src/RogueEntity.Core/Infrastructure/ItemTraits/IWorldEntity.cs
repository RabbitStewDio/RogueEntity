namespace RogueEntity.Core.Infrastructure.ItemTraits
{
    public interface IWorldEntity
    {
        /// <summary>
        ///   A tag is a lookup key for the graphical representation of the entity.
        /// </summary>
        string Tag { get; }
    }
}