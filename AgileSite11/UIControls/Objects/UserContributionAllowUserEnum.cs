using System;

namespace CMS.UIControls
{
    /// <summary>
    /// User contribution allow user enumeration.
    /// </summary>
    public enum UserContributionAllowUserEnum
    {
        /// <summary>
        /// All users are allowed.
        /// </summary> 
        All = 0,

        /// <summary>
        /// Only authenticated users.
        /// </summary>
        Authenticated = 1,

        /// <summary>
        /// Only document owner.
        /// </summary>
        DocumentOwner = 2,
    }
}