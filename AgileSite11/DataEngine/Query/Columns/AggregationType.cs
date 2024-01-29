namespace CMS.DataEngine
{
    /// <summary>
    /// Constants for aggregation types
    /// </summary>
    public enum AggregationType
    {
        /// <summary>
        /// No aggregation
        /// </summary>
        None,
        
        /// <summary>
        /// Number of the items
        /// </summary>
        Count,

        /// <summary>
        /// Sum of the items
        /// </summary>
        Sum,

        /// <summary>
        /// Maximum value
        /// </summary>
        Max,

        /// <summary>
        /// Minimum value
        /// </summary>
        Min,

        /// <summary>
        /// Average value
        /// </summary>
        Average
    }
}
