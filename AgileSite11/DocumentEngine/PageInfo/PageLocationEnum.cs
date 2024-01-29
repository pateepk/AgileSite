namespace CMS.DocumentEngine
{
    /// <summary>
    /// Page location enumeration.
    /// </summary>
    public enum PageLocationEnum : int
    {
        /// <summary>
        /// None location.
        /// </summary>
        None = 0,

        /// <summary>
        /// All (any location).
        /// </summary>
        All = 1,

        /// <summary>
        /// Only secured areas.
        /// </summary>
        SecuredAreas = 2
    }
}