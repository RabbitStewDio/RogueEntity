namespace RogueEntity.Core.Meta.ItemTraits;

/// <summary>
///   Defines whether entity instances should be preserved during map unloading or whether
///   entities should be destroyed unconditionally (and recreated afterwards). 
/// </summary>
public enum EvictionBehaviour
{
    Destroy,
    RemoveAndPreserve
}