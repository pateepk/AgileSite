namespace CMS.Modules
{
    /// <summary>
    /// Element access check status.
    /// </summary>
    public enum ElementAccessCheckStatus
    {
        /// <summary>
        /// No restrictions
        /// </summary>
        NoRestrictions,

        /// <summary>
        /// Element access condition failed
        /// </summary>
        AccessConditionFailed,

        /// <summary>
        /// Automatic read permission check failed
        /// </summary>
        ReadPermissionFailed,

        /// <summary>
        /// UI element permission failed
        /// </summary>
        UIElementAccessFailed,

        /// <summary>
        /// Global application check failed
        /// </summary>
        GlobalApplicationAccessFailed
    }
}
