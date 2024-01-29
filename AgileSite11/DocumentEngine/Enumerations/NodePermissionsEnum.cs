namespace CMS.DocumentEngine
{
    /// <summary>
    /// Node permissions enumeration.
    /// </summary>
    public enum NodePermissionsEnum
    {
        /// <summary>
        /// Read permission.
        /// </summary>
        Read = 0,

        /// <summary>
        /// Modify permission.
        /// </summary>
        Modify = 1,

        /// <summary>
        /// Creates permission.
        /// </summary>
        Create = 2,

        /// <summary>
        /// Delete permission.
        /// </summary>
        Delete = 3,

        /// <summary>
        /// Destroy permission.
        /// </summary>
        Destroy = 4,

        /// <summary>
        /// Explore tree permission.
        /// </summary>
        ExploreTree = 5,

        /// <summary>
        /// Permission to change document permissions.
        /// </summary>
        ModifyPermissions = 6
    }
}