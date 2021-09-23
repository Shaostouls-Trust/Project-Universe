namespace Impact.Utility.ObjectPool
{
    /// <summary>
    /// Defines behavior of object pools when there is no available object.
    /// </summary>
    public enum ObjectPoolFallbackMode
    {
        /// <summary>
        /// Do nothing if no object is available.
        /// </summary>
        None = 0,
        /// <summary>
        /// Steal the object with the lowest priority lower than the current priority.
        /// </summary>
        LowerPriority = 1,
        /// <summary>
        /// Steal the oldest active object (ignores priority).
        /// </summary>
        Oldest = 2,
    }
}

