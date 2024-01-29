using System;
using System.Collections;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Context values shared with web languages helpers.
    /// </summary>
    internal class WebLanguagesContext : AbstractContext<WebLanguagesContext>, INotCopyThreadItem
    {
        #region "Variables"

        private Hashtable mCSSBlocks = null;
        private Hashtable mCSSLinks = null;
        private bool mMinifyCurrentRequest = true;
        private bool mMinifyCurrentRequestScripts = true;
        private Hashtable mClientScriptBlocks = null;
        private Hashtable mStartupScripts = null;
        private Hashtable mHTMLUniqueIDs = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Key-value collection of all registered CSS inline blocks. The value is a flag indicating whether the given CSS block represented by key is registered.
        /// </summary>
        public static Hashtable CurrentCSSBlocks
        {
            get
            {
                if (Current.mCSSBlocks == null)
                {
                    Current.mCSSBlocks = new Hashtable();
                }

                return Current.mCSSBlocks;
            }
        }


        /// <summary>
        /// Key-value collection of all registered CSS links.
        /// </summary>
        public static Hashtable CurrentCSSLinks
        {
            get
            {
                if (Current.mCSSLinks == null)
                {
                    Current.mCSSLinks = new Hashtable();
                }

                return Current.mCSSLinks;
            }
        }


        /// <summary>
        /// If true, the current request links are minified.
        /// </summary>
        public static bool MinifyCurrentRequest
        {
            get
            {
                return Current.mMinifyCurrentRequest;
            }
            set
            {
                Current.mMinifyCurrentRequest = value;
            }
        }


        /// <summary>
        /// If true, the scripts in current request are minified.
        /// </summary>
        public static bool MinifyCurrentRequestScripts
        {
            get
            {
                return Current.mMinifyCurrentRequestScripts;
            }
            set
            {
                Current.mMinifyCurrentRequestScripts = value;
            }
        }


        /// <summary>
        /// Key-value collection of all registered client script blocks. The value is a flag indicating whether the given script represented by key is registered.
        /// </summary>
        public static Hashtable CurrentClientScriptBlocks
        {
            get
            {
                if (Current.mClientScriptBlocks == null)
                {
                    Current.mClientScriptBlocks = new Hashtable();
                }

                return Current.mClientScriptBlocks;
            }
        }


        /// <summary>
        /// Key-value collection of all registered start-up script blocks. The value is a flag indicating whether the given start-up script represented by key is registered.
        /// </summary>
        public static Hashtable CurrentStartupScripts
        {
            get
            {
                if (Current.mStartupScripts == null)
                {
                    Current.mStartupScripts = new Hashtable();
                }

                return Current.mStartupScripts;
            }
        }


        /// <summary>
        /// Key-value collection of all registered HTML identifiers. The value is a flag indicating whether the given HTML ID represented by key is registered.
        /// </summary>
        public static Hashtable CurrentHTMLUniqueIDs
        {
            get
            {
                if (Current.mHTMLUniqueIDs == null)
                {
                    Current.mHTMLUniqueIDs = new Hashtable();
                }

                return Current.mHTMLUniqueIDs;
            }
        }

        #endregion
    }
}

