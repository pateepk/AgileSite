namespace CMS.DataEngine
{
    /// <summary>
    /// Enumeration of the user authorization result.
    /// </summary>
    public enum AuthorizationResultEnum
    {
        /// <summary>
        /// Allowed.
        /// </summary>
        Allowed = 0,

        /// <summary>
        /// Denied.
        /// </summary>
        Denied = 1,

        /// <summary>
        /// Insignificant, does not influence other authorization results.
        /// </summary>
        Insignificant = 2
    }
}