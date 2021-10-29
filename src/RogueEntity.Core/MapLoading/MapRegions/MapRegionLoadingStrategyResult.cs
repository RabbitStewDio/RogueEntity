namespace RogueEntity.Core.MapLoading.MapRegions
{
    public enum MapRegionLoadingStrategyResult
    {
        /// <summary>
        ///    The chunk will be loaded, give it more time. Try your request again.
        /// </summary>
        Pending,
        /// <summary>
        ///    The chunk will be loaded later (but is waiting for some other data),
        ///    there is no point to ask again during this update cycle.
        /// </summary>
        Scheduled,
        /// <summary>
        ///    The chunk is completely loaded.
        /// </summary>
        Success,
        /// <summary>
        ///    There is no such region.
        /// </summary>
        Invalid,
        /// <summary>
        ///     The chunk failed to load at all with an unrecoverable error. Your disk
        ///     probably just crashed.
        /// </summary>
        Error
    }
}
