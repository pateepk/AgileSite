namespace CMS.DataEngine
{
    /// <summary>
    /// Constants for DataQuery operators
    /// </summary>
    public enum OrderDirection
    {
        /// <summary>
        /// Default order direction, keeps the direction the way it is or ascending as default
        /// </summary>
        Default,
        
        /// <summary>
        /// Ascending (A to Z, 0 to 9)
        /// </summary>
        Ascending,

        /// <summary>
        /// Descending (Z to A, 9 to 0)
        /// </summary>
        Descending,
    }
}
