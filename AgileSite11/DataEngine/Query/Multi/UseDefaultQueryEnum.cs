namespace CMS.DataEngine
{
    /// <summary>
    /// Enumeration of the default query usage options
    /// </summary>
    public enum UseDefaultQueryEnum
    {
        /// <summary>
        /// Default query is allowed to be used if necessary
        /// </summary>
        Allowed,

        /// <summary>
        /// Default query should be explicitly used
        /// </summary>
        Force,

        /// <summary>
        /// Default query should never be used
        /// </summary>
        NotAllowed
    }
}