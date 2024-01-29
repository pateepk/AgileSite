namespace CMS.DataEngine
{
    /// <summary>
    /// Represents the site info object identifier.
    /// </summary>
    public class SiteInfoIdentifier : InfoIdentifier
    {
        #region "Constructors"

        /// <summary>
        /// Creates a new site info identifier using the site ID.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public SiteInfoIdentifier(int siteId)
            : base(PredefinedObjectType.SITE, siteId)
        {
        }


        /// <summary>
        /// Creates a new site info identifier using the site name.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public SiteInfoIdentifier(string siteName)
            : base(PredefinedObjectType.SITE, siteName)
        {
        }

        #endregion


        #region "Operators"

        /// <summary>
        /// Implicitly converts integer to SiteInfoIdentifier.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static implicit operator SiteInfoIdentifier(int siteId)
        {
            return (siteId == 0) ? null : new SiteInfoIdentifier(siteId);
        }


        /// <summary>
        /// Implicitly converts string to SiteInfoIdentifier.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static implicit operator SiteInfoIdentifier(string siteName)
        {
            return string.IsNullOrEmpty(siteName) ? null : new SiteInfoIdentifier(siteName);
        }

        #endregion
    }
}