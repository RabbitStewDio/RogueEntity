namespace RogueEntity.Core.Utils.SpatialIndex
{
    /// <summary>
    ///   An internal storage hook to hold a pointer to the next free
    ///   element. Will only be used when there is no element stored.
    ///   Will always contain an negative value when valid. Implementations
    ///   should reuse a field that can never contain negative values.
    /// </summary>
    /// <remarks>
    ///   This is a workaround to express that different data elements
    ///   may have different ways of encoding an empty state that then
    ///   can be reused as cheap reinterpreted storage for the free-pointer.
    ///
    ///   In C=Sharp it is difficult to express union structs safely, the
    ///   next best thing is to hand over control of the data-storage to
    ///   the element itself.
    /// </remarks>
    public interface ISmartFreeListElement<out T> where T: struct, ISmartFreeListElement<T>
    {
        /// <summary>
        ///    If the element is considered empty or uninitialized, returns a pointer to the next free element.
        ///    Otherwise return -1 to indicate that the element is currently in a non-empty state.
        /// </summary>
        FreeListIndex FreePointer { get; }
        
        /// <summary>
        ///   Returns an element that is considered empty, but that stores the given int-value as free-pointer
        ///   data.
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        T AsFreePointer(FreeListIndex ptr);
    }
}
