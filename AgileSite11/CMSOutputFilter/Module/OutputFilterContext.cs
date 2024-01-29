using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Helpers;

namespace CMS.OutputFilter
{
    /// <summary>
    /// Request Output filter values
    /// </summary>
    public class OutputFilterContext : AbstractContext<OutputFilterContext>, INotCopyThreadItem
    {
        #region "Variables"

        private bool? mGZipSuported;
        private bool mLogCurrentOutputToFile;
        private int mCurrentOutputCacheMinutes;
        private int mCurrentNETOutputCacheMinutes;
        private ResponseOutputFilter mCurrentFilter;
        private bool mApplyOutputFilter;
        private bool mOutputFilterEndRequestRequired;
        private bool mCanResolveAllUrls = true;
        private bool mEndRequest = true;
        private bool mFilterResponseOnRender;
        private bool mSentFromCache;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the value that indicates whether response is sent from cache
        /// </summary>
        public static bool SentFromCache
        {
            get
            {
                return Current.mSentFromCache;
            }
            internal set
            {
                Current.mSentFromCache = value;
            }
        }


        /// <summary>
        /// Indicates whether response should be filtered on render (This property should not be used in custom code)
        /// </summary>
        public static bool FilterResponseOnRender
        {
            get
            {
                return Current.mFilterResponseOnRender;
            }
            set
            {
                Current.mFilterResponseOnRender = value;
            }
        }


        /// <summary>
        /// Determines whether resolving of all url can be used.
        /// See HTMLHelper.ResolveUrls method.
        /// </summary>
        public static bool CanResolveAllUrls
        {
            get
            {
                return Current.mCanResolveAllUrls;
            }
            set
            {
                Current.mCanResolveAllUrls = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether end request is required
        /// </summary>
        public static bool OutputFilterEndRequestRequired
        {
            get
            {
                return Current.mOutputFilterEndRequestRequired;
            }
            set
            {
                Current.mOutputFilterEndRequestRequired = value;
            }
        }


        /// <summary>
        /// If true, filter is applied on current request.
        /// </summary>
        public static bool ApplyOutputFilter
        {
            get
            {
                return Current.mApplyOutputFilter;
            }
            set
            {
                Current.mApplyOutputFilter = value;
            }
        }


        /// <summary>
        /// Current request filter.
        /// </summary>
        public static ResponseOutputFilter CurrentFilter
        {
            get
            {
                return Current.mCurrentFilter;
            }
            set
            {
                Current.mCurrentFilter = value;
            }
        }


        /// <summary>
        /// Current output cache minutes for .NET processing
        /// </summary>
        public static int CurrentNETOutputCacheMinutes
        {
            get
            {
                return Current.mCurrentNETOutputCacheMinutes;
            }
            set
            {
                Current.mCurrentNETOutputCacheMinutes = value;
            }
        }



        /// <summary>
        /// Current output cache minutes.
        /// </summary>
        public static int CurrentOutputCacheMinutes
        {
            get
            {
                return Current.mCurrentOutputCacheMinutes;
            }
            set
            {
                Current.mCurrentOutputCacheMinutes = value;
            }
        }


        /// <summary>
        /// If true, current output is logged to the file.
        /// </summary>
        public static bool LogCurrentOutputToFile
        {
            get
            {
                return Current.mLogCurrentOutputToFile;
            }
            set
            {
                Current.mLogCurrentOutputToFile = value;
            }
        }


        /// <summary>
        /// Gets the request value that indicates whether GZip is supported
        /// </summary>
        internal static bool GZipSupported
        {
            get
            {
                if (Current.mGZipSuported == null)
                {
                    Current.mGZipSuported = RequestHelper.IsGZipSupported();
                }
                return Current.mGZipSuported.Value;
            }
        }


        /// <summary>
        /// Indicates whether the request should end after "ProcessCachedOutput" event or not.
        /// Default value is "True".
        /// </summary>
        public static bool EndRequest
        {
            get
            {
                return Current.mEndRequest;
            }
            set
            {
                Current.mEndRequest = value;
            }
        }

        #endregion
    }
}
