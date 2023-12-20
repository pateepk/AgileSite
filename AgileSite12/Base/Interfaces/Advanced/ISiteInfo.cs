namespace CMS.Base
{
    /// <summary>
    /// Objects containing SiteInfo properties.
    /// </summary>
    public interface ISiteInfo : IDataContainer
    {
        #region "Public properties"

        /// <summary>
        /// Site name
        /// </summary>
        string SiteName
        {
            get;
            set;
        }


        /// <summary>
        /// Site ID
        /// </summary>
        int SiteID
        {
            get;
        }

        #endregion
    }
}