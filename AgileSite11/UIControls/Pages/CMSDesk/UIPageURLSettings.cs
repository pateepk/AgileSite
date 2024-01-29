using System.Collections.Specialized;

using CMS.DocumentEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Configuration class used for building UI page URLs.
    /// </summary>
    public class UIPageURLSettings
    {
        #region "Variables"

        private NameValueCollection mQueryParameters = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Node identifier (for the context of current action or displayed document)
        /// </summary>
        public int NodeID
        {
            get;
            set;
        }


        /// <summary>
        /// Node (for the context of current action or displayed document)
        /// </summary>
        public TreeNode Node
        {
            get;
            set;
        }


        /// <summary>
        /// Mode like 'design', 'ediform' etc.
        /// </summary>
        public string Mode
        {
            get;
            set;
        }


        /// <summary>
        /// Action like 'new' etc.
        /// </summary>
        public string Action
        {
            get;
            set;
        }


        /// <summary>
        /// Culture code (for the context of current action)
        /// </summary>
        public string Culture
        {
            get;
            set;
        }


        /// <summary>
        /// Device profile code name
        /// </summary>
        public string DeviceProfile
        {
            get;
            set;
        }


        /// <summary>
        /// Preferred URL to be transformed or updated.
        /// </summary>
        public string PreferredURL
        {
            get;
            set;
        }


        /// <summary>
        /// URL to use as a referential for creating splitview URL.
        /// </summary>
        public string SplitViewSourceURL
        {
            get;
            set;
        }


        /// <summary>
        /// If TRUE, original URL will be transformed to it's comparison variant.
        /// </summary>
        public bool TransformToCompareUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Additional query to append to resulting URL.
        /// </summary>
        public string AdditionalQuery
        {
            get;
            set;
        }


        /// <summary>
        /// Defines whether to allow transformation of URL to splitview page.
        /// </summary>
        public bool AllowSplitview
        {
            get;
            set;
        }


        /// <summary>
        /// Defines whether to allow transformation of URL to ViewValidate page.
        /// </summary>
        public bool AllowViewValidate
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if preferred culture check should be performed.
        /// </summary>
        public bool CheckPreferredCulture
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether the final URL should be protected by hash.
        /// </summary>
        public bool ProtectUsingHash
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether the query string of the current page should be appended to final URL.
        /// </summary>
        public bool AppendCurrentQuery
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if live site URL should be included in result.
        /// </summary>
        public bool IncludeLiveSiteURL
        {
            get;
            set;
        }


        /// <summary>
        /// Collection of query parameters to use during building final URL.
        /// </summary>
        public NameValueCollection QueryParameters
        {
            get
            {
                return mQueryParameters ?? (mQueryParameters = new NameValueCollection());
            }
            set
            {
                mQueryParameters = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor.
        /// </summary>
        public UIPageURLSettings()
        {
            AllowSplitview = true;
            AllowViewValidate = true;
        }

        #endregion
    }
}
