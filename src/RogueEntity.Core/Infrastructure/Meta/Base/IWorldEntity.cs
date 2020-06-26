namespace RogueEntity.Core.Infrastructure.Meta.Base
{
    public interface IWorldEntity
    {
        /// <summary>
        ///   A tag is a lookup key for the graphical representation of the entity.
        /// </summary>
        string Tag { get; }
    }
}