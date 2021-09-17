namespace RogueEntity.Api.ItemTraits
{
    public interface IWorldEntity
    {
        /// <summary>
        ///   A tag is a lookup key for the graphical representation of the entity.
        /// </summary>
        WorldEntityTag Tag { get; }
    }
}