namespace CMS.SiteProvider
{
    /// <summary>
    /// Encapsulates the site name but doesn't request it until it is demanded by Value.
    /// </summary>
    public class SiteNameOnDemand
    {
        private string mValue = null;


        /// <summary>
        /// Value.
        /// </summary>
        public string Value
        {
            get
            {
                // Get site name from context
                if (mValue == null)
                {
                    mValue = SiteContext.CurrentSiteName;
                }

                return mValue;
            }
            set
            {
                mValue = value;
            }
        }


        /// <summary>
        /// Converts the site name to its string representation
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static implicit operator string(SiteNameOnDemand siteName)
        {
            if (siteName == null)
            {
                return null;
            }

            return siteName.Value;
        }
    }
}