namespace CMS.Base
{
    /// <summary>
    /// Default site service
    /// </summary>
    internal class DefaultSiteService : ISiteService
    {
        /// <summary>
        /// Current site
        /// </summary>
        public ISiteInfo CurrentSite
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Returns true, if the current context executes on live site
        /// </summary>
        public bool IsLiveSite
        {
            get
            {
                return true;
            }
        }
    }
}
