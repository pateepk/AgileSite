using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Class containing data for event raised when the settings key is successfuly changed.
    /// </summary>
    public class SettingsKeyChangedEventArgs : CMSEventArgs
    {
        #region "Properties"

        /// <summary>
        /// Gets the name of the key.
        /// </summary>
        /// <value>The name of the key.</value>
        public string KeyName
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets the key value.
        /// </summary>
        /// <value>The key value.</value>
        public object KeyValue
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets the site ID.
        /// </summary>
        /// <value>The site ID.</value>
        public int SiteID
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets the name of the site.
        /// </summary>
        /// <value>The name of the site.</value>
        public string SiteName
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets the action that was performed on the settings key.
        /// </summary>
        /// <value>The action that was performed on the settings key.</value>
        public SettingsKeyActionEnum Action
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets the full name of the key with site name prefix.
        /// </summary>
        /// <value>The full name of the key.</value>
        public string FullKeyName
        {
            get
            {
                if (string.IsNullOrEmpty(SiteName))
                {
                    return KeyName;
                }
                else
                {
                    return string.Concat(SiteName, ".", KeyName);
                }
            }
        }

        #endregion


        #region "Constructors"
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsKeyChangedEventArgs"/> class.
        /// </summary>
        protected SettingsKeyChangedEventArgs()
        {
            KeyName = string.Empty;
            KeyValue = new object();
            SiteID = 0;
            SiteName = string.Empty;
            Action = SettingsKeyActionEnum.Update;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsKeyChangedEventArgs"/> class.
        /// </summary>
        /// <param name="keyName">Name of the key</param>
        /// <param name="keyValue">The key value</param>
        /// <param name="siteId">ID of the site</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="action">Action that was performed on the settings key</param>
        public SettingsKeyChangedEventArgs(string keyName, object keyValue, int siteId, string siteName, SettingsKeyActionEnum action)
        {
            KeyName = keyName;
            KeyValue = keyValue;
            SiteID = siteId;
            SiteName = siteName;
            Action = action;
        }

        #endregion
    }
}