using CMS.SiteProvider;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Base class for translation service interface.
    /// </summary>
    public abstract class BaseTranslationService
    {
        #region "Private variables"

        private string mSiteName;

        #endregion


        #region "Properties"

        /// <summary>
        /// Get or sets custom parameter.
        /// </summary>
        public object CustomParameter
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the name of the site where the settings are retrieved from.
        /// </summary>
        public virtual string SiteName
        {
            get
            {
                return mSiteName ?? SiteContext.CurrentSiteName;
            }
            protected set
            {
                mSiteName = value;
            }
        }

        #endregion


        #region "Abstract methods"

        /// <summary>
        /// Checks if everything required to run the service is in the settings of the service.
        /// </summary>
        public abstract bool IsAvailable();

        #endregion
    }
}
