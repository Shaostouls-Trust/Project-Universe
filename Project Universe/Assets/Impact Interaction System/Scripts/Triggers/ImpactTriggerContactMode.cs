namespace Impact.Triggers
{
    /// <summary>
    /// Specifies the different contact point modes for collision-based triggers.
    /// </summary>
    public enum ImpactTriggerContactMode
    {
        /// <summary>
        /// Play interactions at only a single contact point.
        /// </summary>
        Single = 0,

        /// <summary>
        /// Play interactions at a single point that is the average of multiple contact points.
        /// This will average both the contact position and the contact normal.
        /// </summary>
        SingleAverage = 1,

        /// <summary>
        /// Play interactions at a every contact point of a collision.
        /// </summary>
        Multiple = 2
    }
}
